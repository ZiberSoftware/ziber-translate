(function() {
    'use strict';
    
    angular.module('Translate.Services')
        .factory('AuthenticationService', ['$http', 'SessionService', function ($http, SessionService) {
            return {
                login: function(user) {
                    SessionService.currentUser = user;
                },
                isLoggedIn: function() {
                    return SessionService.currentUser !== null;
                },
                logout: function () {
                    SessionService.currentUser = null;
                },
                translatorInfo: function() {
                    return $http.get('/Security/TranslatorRank').then(function (response) {
                        return response.data;
                    });
                }
            };
        }])
        .factory('SessionService', [function() {
            return {
                currentUser: null
            };
        }]);
})();