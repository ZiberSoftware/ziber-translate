(function () {
    'use strict';


    angular.module('Translate.Controllers', ['Translate.Services'])
        .controller('SetsCtrl', ['$scope', 'TranslationService', function ($scope, service) {
            service.sets().then(function (sets) {
                $scope.sets = sets;
            });

            $scope.language = 'nl';
        }])
        .controller('TranslationCtrl', ['$scope', '$routeParams', 'TranslationService', 'ChangesetService', function ($scope, $routeParams, service, changesetService) {
            service.translations($routeParams['id'], $routeParams['language']).then(function (translations) {
                $scope.translations = translations;
            });

            $scope.$on('translationChanged', function (e, translation, newValue) {
                translation.Value = newValue;
                service.update(translation);
            });

            $scope.$on('leftEditMode', function (e, $element, prevOrNext) {
                //don't run this during $apply cycle
                setTimeout(function () {
                    var next = $element.parent()[prevOrNext === 'prev' ? 'prev' : 'next']();
                    next.find('.translation .edit').trigger('click');
                }, 0);
            });
        }]);


})();