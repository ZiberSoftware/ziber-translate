(function() {
    'use strict';


    angular.module('Translate.Controllers')
        .controller('HeaderCtrl', ['$scope', '$http', '$location', 'SessionService', 'AuthenticationService', function ($scope, $http, $location, SessionService, authService) {            
            $scope.user = SessionService.currentUser;
            $scope.isAuthenticated = false;
            $scope.login = function () {
                $location.search('redirectUrl', $location.path()).path('/login');
            };
            $scope.$watch('user', function () {
                if ($scope.user != null) {
                    $scope.isAuthenticated = true;
                }
                else
                    $scope.isAuthenticated = false;
            }, true);
           

            $scope.logout = function () {
                $http.post('/Security/Logout')
                    .succes(function () {
                        authService.logout();
                        location.path('/');                      
                    });
            };

            console.log($scope.user, SessionService)
        }]);
})();