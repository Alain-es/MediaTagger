angular.module('umbraco.resources').factory('MediaTaggerResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {

        getPagedMedias: function (tags, pageNumber, pageSize) {
            return $http.get("backoffice/MediaTagger/MediaTaggerApi/GetPagedMedias", {
                params: { tags: tags, pageNumber: pageNumber, pageSize: pageSize }
            });
        },

        getPagedMediasHasMoreResults: function (tags, pageNumber, pageSize) {
            return $http.get("backoffice/MediaTagger/MediaTaggerApi/GetPagedMediasHasMoreResults", {
                params: { tags: tags, pageNumber: pageNumber, pageSize: pageSize }
            });
        }

    };
})

