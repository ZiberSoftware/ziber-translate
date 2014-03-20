(function () {
    'use strict';
    
    angular.module('Translate.Services')
        .factory('TranslationService', ['$http', function ($http) {
            return {
                sets: function () {
                    return $http.get('/TranslateSet').then(function (response) {
                        return response.data;
                    });
                },
                translations: function (setId, language, filter, page) {
                    if (typeof(filter) === 'undefined')
                        filter = 'all';
                    
                    if (typeof(page) === 'undefined') {
                        page = 1;
                    }
                    
                    return $http.get('/sets/' + setId + '/translations-' + language + '/filter-' + filter + '?pageNr=' + page).then(function (response) {
                        response.data.translations.forEach(function (item) {
                            item.approved = item.Votes > 0;
                            item.voted = item.Voted;
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
                submit: function () {
                    return $http.post('/ChangeSet/Submit');
                },
                get: function() {
                    return $http.get('/ChangeSet/').then(function(result) {
                        return result.data;
                    });
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