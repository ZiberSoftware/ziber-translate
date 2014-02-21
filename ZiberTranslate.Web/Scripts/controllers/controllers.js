(function () {
    'use strict';


    angular.module('Translate.Controllers', ['Translate.Services'])
        .controller('SetsCtrl', ['$scope', 'TranslationService', function($scope, service) {
            service.sets().then(function(sets) {
                $scope.sets = sets;
            });

            $scope.language = 'nl';
        }])
        .controller('TranslationCtrl', ['$scope', '$routeParams', '$http', 'TranslationService', function($scope, $routeParams, $http, service) {
            $http.get('/Translation/Filters?setId=' + $routeParams['id'] + '&language=' + $routeParams['language']).then(function(result) {
                $scope.filters = result.data;
            });

            $scope.$on('translationChanged', function(e, translation, newValue) {
                translation.Value = newValue;
                service.update(translation);
            });

            $scope.filter = function(filter) {
                service.translations($routeParams['id'], $routeParams['language'], filter).then(function(translations) {
                    $scope.translations = translations;
                });
            };

            $scope.filter('all');

            $scope.$on('leftEditMode', function(e, $element, prevOrNext) {
                //don't run this during $apply cycle
                setTimeout(function() {
                    var next = $element.parent()[prevOrNext === 'prev' ? 'prev' : 'next']();
                    next.find('.translation .edit').trigger('click');
                }, 0);
            });
        }])
        .controller('ChangeSetCtrl', ['$scope', '$http', function ($scope, $http) {
            $http.get('/ChangeSet').then(function(response) {
                $scope.changes = response.data.changes;
                $scope.Votes = response.data.votes;
            });
        }]);
})();