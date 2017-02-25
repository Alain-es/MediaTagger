'use strict';
(function () {

    //Main controller
    function MediaTaggerDashboardController($q, $rootScope, $scope, assetsService, mediaResource, MediaTaggerResource, umbRequestHelper, angularHelper, $timeout, $element) {

        // Consts
        var tagGroupName = "default"; //"mediaTagger";
        var pageSize = 20;

        var $typeahead;

        $scope.isLoading = true;
        $scope.tagToAdd = "";
        $scope.images = [];
        $scope.areThereMoreResultsButton = false;
        $scope.currentPage = 0;

        // Load the typeahead library 
        var umbracoVersion = Umbraco.Sys.ServerVariables.application.version;
        var await = [];
        if (umbracoVersion < "7.2.2") {
            await.push(assetsService.loadJs('/umbraco/lib/typeahead/typeahead.bundle.min.js', $scope));
        }
        else if (umbracoVersion < "7.3.0") {
            await.push(assetsService.loadJs('/umbraco/lib/typeahead-js/typeahead.bundle.min.js', $scope));
        }
        else {
            await.push(assetsService.loadJs('/umbraco/lib/typeahead.js/typeahead.bundle.min.js', $scope));
        }
        $q.all(await).then(function () {

            $scope.isLoading = false;
            $scope.currentTags = [];

            // Get all the images for the selected tags
            function refreshResults(addToCurrentResult) {
                if (addToCurrentResult != true) {
                    $scope.areThereMoreResultsButton = false;
                    $scope.currentPage = 0;
                }
                var tags = $scope.currentTags.join();
                MediaTaggerResource.getPagedMedias(tags, $scope.currentPage, pageSize).then(
                        function (response) {
                            if (response.data.items) {
                                if (addToCurrentResult != true) {
                                    $scope.images = response.data.items;
                                }
                                else {
                                    $scope.images = $scope.images.concat(response.data.items);
                                }
                            }
                            else {
                                $scope.images = [];
                            }
                        },
                        function (error) {
                            console.log(error);
                        }
                    );
                MediaTaggerResource.getPagedMediasHasMoreResults(tags, $scope.currentPage, pageSize).then(
                        function (response) {
                            $scope.areThereMoreResultsButton = response.data;
                        },
                        function (error) {
                            console.log(error);
                        }
                    );

                // That is necessary in order to allow images to be resized automatically
                $('.umb-photo-folder').css({ "width": "" });
            }

            //init load
            refreshResults();


            $scope.showMoreResults = function () {
                $scope.currentPage++;
                refreshResults(true);
            };


            // Helper method to add a tag on enter or on typeahead select
            // And retrieve medias for the selected tags
            function addTag(tagToAdd) {
                if (tagToAdd.length > 0) {
                    if ($scope.currentTags.indexOf(tagToAdd) < 0) {
                        $scope.currentTags.push(tagToAdd);
                    }
                    refreshResults();
                }
            }

            $scope.addTagOnEnter = function (e) {
                var code = e.keyCode || e.which;
                if (code == 13) { //Enter keycode   
                    if ($element.find('.tags-searchbox').parent().find(".tt-dropdown-menu .tt-cursor").length === 0) {
                        //this is required, otherwise the html form will attempt to submit.
                        e.preventDefault();
                        $scope.addTag();
                    }
                }
            };

            $scope.addTag = function () {
                //ensure that we're not pressing the enter key whilst selecting a typeahead value from the drop down
                //we need to use jquery because typeahead duplicates the text box
                addTag($scope.tagToAdd);
                $scope.tagToAdd = "";
                //this clears the value stored in typeahead so it doesn't try to add the text again
                // http://issues.umbraco.org/issue/U4-4947
                $typeahead.typeahead('val', '');
            };

            $scope.removeTag = function (tag) {
                var i = $scope.currentTags.indexOf(tag);
                if (i >= 0) {
                    $scope.currentTags.splice(i, 1);
                    refreshResults();
                }
            };

            //configure the tags data source

            //helper method to format the data for bloodhound
            function dataTransform(list) {
                //transform the result to what bloodhound wants
                var tagList = _.map(list, function (i) {
                    return { value: i.text };
                });
                // remove current tags from the list
                return $.grep(tagList, function (tag) {
                    return ($.inArray(tag.value, $scope.currentTags) === -1);
                });
            }

            // helper method to remove current tags
            function removeCurrentTagsFromSuggestions(suggestions) {
                return $.grep(suggestions, function (suggestion) {
                    return ($.inArray(suggestion.value, $scope.currentTags) === -1);
                });
            }

            var tagsHound = new Bloodhound({
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                dupDetector: function (remoteMatch, localMatch) {
                    return (remoteMatch["value"] == localMatch["value"]);
                },
                //pre-fetch the tags for this category
                prefetch: {
                    url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", [{ tagGroup: tagGroupName }]),
                    //TTL = 5 minutes
                    ttl: 300000,
                    filter: dataTransform
                },
                //dynamically get the tags for this category (they may have changed on the server)
                remote: {
                    url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", [{ tagGroup: tagGroupName }]),
                    filter: dataTransform
                }
            });

            tagsHound.initialize(true);

            //configure the type ahead
            $timeout(function () {

                $typeahead = $element.find('.tags-searchbox').typeahead(
                {
                    //This causes some strangeness as it duplicates the textbox, best leave off for now.
                    hint: false,
                    highlight: true,
                    cacheKey: new Date(),  // Force a cache refresh each time the control is initialized
                    minLength: 1
                }, {
                    //see: https://github.com/twitter/typeahead.js/blob/master/doc/jquery_typeahead.md#options
                    // name = the data set name, we'll make this the tag group name
                    name: tagGroupName,
                    displayKey: "value",
                    //source: tagsHound.ttAdapter(),
                    source: function (query, cb) {
                        tagsHound.get(query, function (suggestions) {
                            cb(removeCurrentTagsFromSuggestions(suggestions));
                        });
                    },
                }).bind("typeahead:selected", function (obj, datum, name) {
                    angularHelper.safeApply($scope, function () {
                        addTag(datum["value"]);
                        $scope.tagToAdd = "";
                        // clear the typed text
                        $typeahead.typeahead('val', '');
                    });

                }).bind("typeahead:autocompleted", function (obj, datum, name) {
                    angularHelper.safeApply($scope, function () {
                        addTag(datum["value"]);
                        $scope.tagToAdd = "";
                    });

                }).bind("typeahead:opened", function (obj) {
                    //console.log("opened ");
                });
            });

            $scope.$on('$destroy', function () {
                tagsHound.clearPrefetchCache();
                tagsHound.clearRemoteCache();
                $element.find('.tags-searchbox').typeahead('destroy');
                delete $scope.tagsHound;
            });

        });

    };

    //register the controller
    angular.module("umbraco").controller('MediaTagger.Dashboard.MediaTaggerDashboardController', MediaTaggerDashboardController);

})();



