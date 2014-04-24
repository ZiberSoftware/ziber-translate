(function () {
    'use strict';

    $.fn.setCaretPosition = function (pos) {
        return this.each(function (index, elem) {
            if (elem.setSelectionRange) {
                elem.focus();
                elem.setSelectionRange(pos, pos);
            } else if (elem.createTextRange) {
                var range = elem.createTextRange();
                range.collapse(true);
                range.moveEnd('character', pos);
                range.moveStart('character', pos);
                range.select();
            }
        });
    };

    angular.module('Translate.Directives')
        .directive('zbrAdminTranslation', ['$rootScope', '$sce', 'AdminService', function ($rootScope, $sce, AdminService) {
            return {
                restrict: 'AE',
                template:
                    '<div class=\'translation-root\' bindonce>' +
                        '<div class=\'term\' bo-text=\'translation.Term\'></div>' +
                        '<div class=\'spacer\'>' +
                            '<span>' +
                                '<span class="vote-count" ng-bind="translation.Votes" ng-if="translation.Votes > 0"></span>' +
                                '<span class="vote-count" ng-bind="translation.UserVotes" ng-if="translation.UserVotes > 0"></span>' +
                            '</span>' +
                        '</div>' +
                        '<div class=\'translation\'>' +
                            '<p class=\'values\' ng-bind-html="valueAsHtml" ng-bind=\'value\'></p>' +                          
                        '</div>' +
                        '<div class =\'userinfo\'>' +
                            '<p class =\'translatorname\' ng-bind="translation.TranslatorName"></p>' +
                        '</div>' +
                        '<div class=\'adminCheckbox\'>' +
                            '<div class=\'adminApproval\'>' +
                                '<input type="checkbox" value="translation.TranslationId" ng-model="translation.TranslationId" name="translationId" />' +
                            '</div>' +
                        '</div>' +
                    '</div>',
                scope: {
                    translation: '='
                },
                replace: true,
                link: function ($scope, $attr) {
                    $scope.value = $scope.translation.Value;

                    $scope.$watch('value', function (newValue, oldValue) {
                       
                        if (newValue != $scope.translation.LeadingValue) {
                            $scope.valueAsHtml = $sce.trustAsHtml(diffString($scope.translation.LeadingValue, newValue));
                        }
                      
                    });                   
                }
            };
        }]);
})();