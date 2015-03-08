'use strict';
(function () {

    //Main controller

    function MediaTaggerPickerController($rootScope, $scope, $routeParams, dialogService, entityResource, mediaResource, mediaHelper, $timeout, userService, MediaTaggerResource, searchService) {

        // Private folder 
        if ($scope.model.config.restrictPrivateFolder === '1') {
            var contentId = $routeParams.id;
            var startMediaId = $scope.model.config.startMediaId;
            if (!startMediaId)
                startMediaId = -1;
            // Sets startMediaId with an Id that doesn't exist in order to return no result in case the privateFolder is not found
            $scope.model.config.startMediaId = -9999;
            // Search media folder with a name that matches contentID
            searchService.searchMedia({ term: contentId }).then(function (data) {
                if (data) {
                    // Check whether the folder's parent node is the StartMediaId node
                    var privateFolderNode = _.find(data, function (mediaNode) {
                        var path = mediaNode.path.split(",");
                        if (path[path.length - 2] == startMediaId)
                            return mediaNode;
                    });
                    if (privateFolderNode)
                        $scope.model.config.startMediaId = privateFolderNode.id;
                }
            });
        }

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker && $scope.model.config.multiPicker !== '0' ? true : false;

        function setupViewModel() {
            $scope.images = [];
            $scope.ids = [];

            if ($scope.model.value) {
                var ids = $scope.model.value.split(',');

                //NOTE: We need to use the entityResource NOT the mediaResource here because
                // the mediaResource has server side auth configured for which the user must have
                // access to the media section, if they don't they'll get auth errors. The entityResource
                // acts differently in that it allows access if the user has access to any of the apps that
                // might require it's use. Therefore we need to use the metatData property to get at the thumbnail
                // value.

                entityResource.getByIds(ids, "Media").then(function (medias) {

                    _.each(medias, function (media, i) {

                        //only show non-trashed items
                        if (media.parentId >= -1) {

                            if (!media.thumbnail) {
                                media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                            }

                            $scope.images.push(media);
                            $scope.ids.push(media.id);
                        }
                    });

                    $scope.sync();
                });
            }
        }

        setupViewModel();

        $scope.remove = function (index) {
            $scope.images.splice(index, 1);
            $scope.ids.splice(index, 1);
            $scope.sync();
        };

        $scope.add = function () {

            $scope.mediaPicker = function (options) {
                options.template = '/App_Plugins/MediaTagger/Dialog/_dialog.html';
                options.show = true;
                return dialogService.open(options);
            };


            $scope.mediaPicker({
                startMediaId: $scope.model.config.startMediaId,
                multiPicker: multiPicker,
                restrictPrivateFolder: $scope.model.config.restrictPrivateFolder === '1' ? true : false,
                callback: function (data) {

                    //it's only a single selector, so make it into an array
                    if (!multiPicker) {
                        data = [data];
                    }

                    _.each(data, function (media, i) {

                        if (!media.thumbnail) {
                            media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                        }

                        $scope.images.push(media);
                        $scope.ids.push(media.id);
                    });

                    $scope.sync();
                }
            });



            //dialogService.mediaPicker({
            //    startNodeId: $scope.model.config.startNodeId,
            //    multiPicker: multiPicker,
            //    callback: function (data) {

            //        //it's only a single selector, so make it into an array
            //        if (!multiPicker) {
            //            data = [data];
            //        }

            //        _.each(data, function (media, i) {

            //            if (!media.thumbnail) {
            //                media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
            //            }

            //            $scope.images.push(media);
            //            $scope.ids.push(media.id);
            //        });

            //        $scope.sync();
            //    }
            //});
        };

        $scope.sortableOptions = {
            update: function (e, ui) {
                var r = [];
                //TODO: Instead of doing this with a half second delay would be better to use a watch like we do in the 
                // content picker. THen we don't have to worry about setting ids, render models, models, we just set one and let the 
                // watch do all the rest.
                $timeout(function () {
                    angular.forEach($scope.images, function (value, key) {
                        r.push(value.id);
                    });

                    $scope.ids = r;
                    $scope.sync();
                }, 500, false);
            }
        };

        $scope.sync = function () {
            $scope.model.value = $scope.ids.join();
        };

        $scope.showAdd = function () {
            if (!multiPicker) {
                if ($scope.model.value && $scope.model.value !== "") {
                    return false;
                }
            }
            return true;
        };

        //here we declare a special method which will be called whenever the value has changed from the server
        //this is instead of doing a watch on the model.value = faster
        $scope.model.onValueChanged = function (newVal, oldVal) {
            //update the display val again if it has changed from the server
            setupViewModel();
        };

    };


    //register the controller
    angular.module("umbraco").controller('MediaTagger.Picker.MediaTaggerPickerController', MediaTaggerPickerController);

})();





