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
                }
            };
        }])
        .factory('SessionService', [function() {
            return {
                currentUser: null
            };
        }]);
})();