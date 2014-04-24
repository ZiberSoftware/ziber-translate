(function () {
    'use strict';


    angular.module('Translate.Controllers')
        .controller('SetsCtrl', ['$scope', 'TranslationService', function($scope, service) {
            service.sets().then(function(sets) {
                $scope.sets = sets;
            });

            $scope.language = 'nl';
        }])
        .controller('TranslationCtrl', ['$scope', '$routeParams', '$http', '$location', 'TranslationService', 'AuthenticationService', function ($scope, $routeParams, $http, $location, service, authService) {

            $http.get('/Translation/Filters?setId=' + $routeParams['id'] + '&language=' + $routeParams['language']).then(function (result) {
                $scope.filters = result.data;
            });

            //fires when a translation for a user changes
            $scope.$on('translationChanged', function(e, translation, newValue) {
                translation.Value = newValue;
                service.update(translation).then(function (result) {
                    var changeSet = result.data;

                    $scope.$broadcast('changesetUpdated', changeSet);
                });
            });

            $scope.currentFilter = 'all';            

            $scope.$watch('currentFilter', function () {
                
                $scope.filter($scope.currentFilter);
            }, true);            

            $scope.$on('changesetUpdated', function (changeset) {
                $scope.filter($scope.currentFilter);
            });
            

            var range = function (i) {
                return i ? range(i - 1).concat(i) : [];
            }

            $scope.gotoPage = function (page) {
                $location.path('/sets/' + $routeParams['id'] + '/translations/' + $routeParams['language'] + '/' + page);
            };

            $scope.filter = function(filter) {
                service.translations($routeParams['id'], $routeParams['language'], filter, $routeParams["page"]).then(function (data) {
                    $scope.translations = data.translations;
                    $scope.totalPages = data.totalPages;
                    $scope.currentPage = data.currentPage;
                    $scope.pages = range(data.totalPages);                    
                });
            };

            $scope.checkUser = function () {
                if (!authService.isLoggedIn()) {
                    window.location = '/Security/Login?redirectUrl=' + encodeURIComponent('/#' + $location.path());
                }
            };
            
            $scope.filter($scope.currentFilter);            

            $scope.$on('leftEditMode', function(e, $element, prevOrNext) {
                //don't run this during $apply cycle
                setTimeout(function() {
                    var next = $element.parent()[prevOrNext === 'prev' ? 'prev' : 'next']();
                    next.find('.translation .edit').trigger('click');
                }, 0);
            });
        }])
        .controller('ChangeSetSummaryCtrl', ['$scope', '$http', function ($scope, $http) {

            $scope.$watch('[changes, votes]', function () {
                $scope.totalChanges = $scope.changes + $scope.votes;
            }, true);
            
            $http.get('/Translation/CountChanges').then(function (response) {
                $scope.changes = response.data.changes;
                $scope.votes = response.data.votes;
            });

            $scope.$on('changesetUpdated', function (event, changeSet) {
                $scope.changes = changeSet.changes;
                $scope.votes = changeSet.votes;                
            });
        }])
        .controller('AdminSetCtrl', ['$scope', 'AdminService', function ($scope, AdminService) {

            AdminService.reviewSets().then(function (sets) {
                $scope.sets = sets.setsWithReviews.ChangedSets;
            });           
        }])
        .controller('AdminTranslationCtrl', ['$rootScope', '$scope', '$http', '$routeParams', '$location', 'AdminService', function ($rootScope, $scope, $http, $routeParams, $location, AdminService) {           

            AdminService.reviewTranslations($routeParams['id'], $routeParams['language']).then(function (data) {
                $scope.translations = data.setContent;
            });
        }])
        .controller('InitCtrl', ['UserConfig', 'AuthenticationService', function(userconfig, authService) {
            if (userconfig.authenticated) {
                authService.login(userconfig.user);
            }
        }]);
})();