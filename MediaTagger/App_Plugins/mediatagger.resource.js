angular.module('umbraco.resources').factory('MediaTaggerResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {

        getPagedMedias: function (tags, pageNumber, pageSize, startMediaId, restrictPrivateFolder) {
            return $http.get("backoffice/MediaTagger/MediaTaggerApi/GetPagedMedias", {
                params: { tags: tags, pageNumber: pageNumber, pageSize: pageSize, startMediaId: startMediaId, restrictPrivateFolder: restrictPrivateFolder }
            });
        },

        getPagedMediasHasMoreResults: function (tags, pageNumber, pageSize, startMediaId, restrictPrivateFolder) {
            return $http.get("backoffice/MediaTagger/MediaTaggerApi/GetPagedMediasHasMoreResults", {
                params: { tags: tags, pageNumber: pageNumber, pageSize: pageSize, startMediaId: startMediaId, restrictPrivateFolder: restrictPrivateFolder }
            });
        }

    };
})

