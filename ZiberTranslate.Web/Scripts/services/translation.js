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
                update: function (translation, language) {
                    return $http.post('sets/' + translation.SetId + '/translations-' + language + '/' + translation.KeyId + '/update', { value: translation.Value });
                },
                approve: function (translation, language) {
                    return $http.post('sets/' + translation.SetId + '/translations-' + language + '/' + translation.KeyId + '/approve', { value: translation.Value });
                },
                disapprove: function (translation, language) {
                    return $http.post('sets/' + translation.SetId + '/translations-' + language + '/' + translation.KeyId + '/disapprove', { value: translation.Value });
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
                },
                cancel: function () {
                return $http.post('/ChangeSet/CancelChanges');
                }
            };
        }])
        .factory('AdminService', ['$http', function ($http) {
            return {
                reviewSets: function () {
                    return $http.get('/Admin').then(function (result) {
                        return result.data;
                    });
                },
                getReviewTranslations: function (setId, language) {
                    return $http.get('/admin/' + setId + '/review-translations-' + language).then(function (response) {
                        return response.data;
                    });
                },
                removeTranslations: function (setId, language, translationId) {
                    return $http.post('/Admin/Remove?language=' + language + '&setId=' + setId + '&translationId=' + translationId);
                },
                acceptKeepTranslations: function (setId, language, translationId) {
                    return $http.post('/Admin/AcceptKeep?language=' + language + '&setId=' + setId + '&translationId=' + translationId);
                },
                acceptRemoveTranslations: function (setId, language, translationId) {
                    if (translationId.length == 0)
                        translationId = null;
                    return $http.post('/Admin/AcceptRemove?language=' + language + '&setId=' + setId + '&translationId=' + translationId);
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