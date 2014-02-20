(function () {
    'use strict';


    angular.module('Translate.Services', [])
        .factory('TranslationService', ['$http', function ($http) {
            return {
                sets: function () {
                    return $http.get('/TranslateSet').then(function (response) {
                        return response.data;
                    });
                },
                translations: function (setId, language) {
                    return $http.get('/sets/' + setId + '/translations-' + language + '/filter-all').then(function (response) {
                        response.data.forEach(function (item) {
                            item.approved = item.Votes > 0;
                            item.voted = item.Votes > 0;
                        });
                        return response.data;
                    });
                },
                update: function (translation) {
                    return $http.post('sets/' + translation.SetId + '/translations-nl/' + translation.KeyId + '/update', { value: translation.Value });
                },
                approve: function (translation) {
                    return $http.post('sets/' + translation.SetId + '/translations-nl/' + translation.KeyId + '/approve', { value: translation.Value });
                }
            };
        }])
        .factory('ChangesetService', ['$http', function ($http) {
            return {
                approve: function () {

                }
            };
        }])
        .factory('mvcHttpInterceptor', function ($q) {
            return {
                request: function (request) {
                    request.headers['X-Requested-With'] = 'XMLHttpRequest';

                    return request;
                }
            };
        });
})();