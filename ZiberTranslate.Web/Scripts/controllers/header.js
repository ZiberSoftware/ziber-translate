(function() {
    'use strict';


    angular.module('Translate.Controllers')
        .controller('HeaderCtrl', ['$scope', '$http', '$location', 'SessionService', 'AuthenticationService', function ($scope, $http, $location, SessionService, authService) {            
            
            $scope.user = SessionService.currentUser;
            $scope.isAuthenticated = false;
            
             $scope.translator = function () {
                authService.translatorInfo().then(function (data) {
                    $scope.translatorRank = data.rank;
                });
            };

            $scope.login = function () {
                $location.search('redirectUrl', $location.path()).path('/login');
            };

            if ($scope.user != null) 
                $scope.isAuthenticated = true;          
            else
                $scope.isAuthenticated = false;           
           
            $scope.translator();
            $scope.isAdminRank = true;
            
            $scope.logout = function () {
                $http.post('/Security/Logout')
                    .then(function () {
                        authService.logout();
                        $location.path('/');
                    });
            };

            console.log($scope.user, SessionService);
            console.log($scope.isAuthenticated);
            console.log($scope.isAdminRank);
        }]);
})();