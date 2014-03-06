(function() {
    'use strict';


    angular.module('Translate.Controllers')
        .controller('HeaderCtrl', ['$scope', 'SessionService', function ($scope, SessionService) {
            $scope.user = SessionService.currentUser;
            console.log($scope.user, SessionService)
        }]);
})();