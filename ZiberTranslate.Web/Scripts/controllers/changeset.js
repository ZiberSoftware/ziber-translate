(function () {
    'use strict';

    angular.module('Translate.Controllers')
        .controller('ChangeSetCtrl', ['$rootScope', '$scope', '$routeParams', '$location', 'ChangesetService', function ($rootScope, $scope, $routeParams, $location, ChangesetService) {
            ChangesetService.get().then(function (changeset) {
                $scope.translations = changeset.changes;
            });

            $scope.submit = function () {
                ChangesetService.submit().then(function () {
                    $location.path('/');
                });
            };

            $scope.cancel = function () {
                ChangesetService.cancel().then(function () {
                    $rootScope.$broadcast('changesetUpdated', { changes: 0, votes: 0 });
                });
            };
        }]);
})();