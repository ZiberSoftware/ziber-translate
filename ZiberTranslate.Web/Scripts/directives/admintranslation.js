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

    angular.module('Translate.Directives', ['pasvaz.bindonce'])
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
                            '<p ng-show=\'asHtml && !editMode\' class="edit" ng-bind-html="valueAsHtml" ng-bind=\'value\' ng-click=\'editMode = true; editingValue = value\'></p>' +
                            '<p ng-hide=\'asHtml || editMode\' class="edit" ng-bind="value" ng-bind=\'value\' ng-click=\'editMode = true; editingValue = value\'></p>' +
                            '<div class="field" ng-show=\'editMode\'>' +
                                '<textarea class="inline-edit" autocomplete="off" ng-model="editingValue" ng-keydown=\'editorKeyDown($event)\'></textarea>' +
                            '</div>' +
                        '</div>' +     
                        '<div class=\'admin\'>' +
                            '<span class=\'adminApproval\'>' +
                                '<input type="checkbox" value="translation.Id" name="translationId" />' +
                            '</span>' +
                    '</div>',
                scope: {
                    translation: '='
                },
                replace: true,
                link: function ($scope, $element, $attr) {
                    $scope.value = $scope.translation.Value;

                    var initial = true;
                    $scope.$watch('editMode', function (newValue) {
                        if (newValue) {
                            var key = $element.find('.term');
                            var editor = $element.find('textarea');
                            editor.css('height', key.height()).setCaretPosition(0);
                        }
                    });

                    $scope.$watch('value', function (newValue, oldValue) {
                        if (!initial) {
                            $rootScope.$broadcast('translationChanged', $scope.translation, newValue);
                        }

                        if (newValue != $scope.translation.LeadingValue) {
                            $scope.valueAsHtml = $sce.trustAsHtml(diffString($scope.translation.LeadingValue, newValue));
                            $scope.asHtml = true;

                            initial = false;
                        } else {
                            $scope.asHtml = false;
                            initial = false;
                        }
                    });

                    $scope.exitEditMode = function () {
                        $scope.editMode = false;
                        $scope.value = $scope.editingValue;
                    };

                    
                }
            };
        }]);
})();