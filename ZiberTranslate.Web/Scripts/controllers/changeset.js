(function() {
    'use strict';

    angular.module('Translate.Controllers')
        .controller('ChangeSetCtrl', ['$scope', '$location', 'ChangesetService', function ($scope, $location, ChangesetService) {
            ChangesetService.get().then(function (changeset) {
                $scope.translations = changeset.changes;
            });

            $scope.submit = function() {
                ChangesetService.submit().then(function() {
                    $location.path('/');
                });
            };
        }]);
})();