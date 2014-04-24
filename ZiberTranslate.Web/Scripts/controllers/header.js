(function() {
    'use strict';


    angular.module('Translate.Controllers')
        .controller('HeaderCtrl', ['$scope', '$http', '$location', 'SessionService', function ($scope, $http, $location, SessionService) {            
            
            $scope.user = SessionService.currentUser;
            
             $scope.login = function () {
                 window.location = '/Security/Login?redirectUrl=' + encodeURIComponent('/#' + $location.path());
            };

            if ($scope.user != null) 
                $scope.isAuthenticated = true;          
            else
                $scope.isAuthenticated = false;           
                       
            $scope.logout = function () {
                window.location = '/Security/Logout';
            };

            console.log($scope.user, SessionService);
            console.log($scope.isAuthenticated);
        }]);
})();