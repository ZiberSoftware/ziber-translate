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

            $http.get('/Translation/Filters?setId=' + $routeParams['id'] + '&language=' + $routeParams['language']).then(function(result) {
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


            $scope.filter = function(filter) {
                service.translations($routeParams['id'], $routeParams['language'], filter, $routeParams["page"]).then(function(translations) {
                    $scope.translations = translations;
                });
            };

            $scope.checkUser = function () {
                if (!authService.isLoggedIn()) {
                    $location.search('redirectUrl', $location.path()).path('/login');
                }
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

            $scope.$watch('[changes, votes]', function () {
                $scope.totalChanges = $scope.changes + $scope.votes;
            }, true);
            
            $http.get('/ChangeSet').then(function(response) {
                $scope.changes = response.data.changes;
                $scope.votes = response.data.votes;
            });

            $scope.$on('changesetUpdated', function (event, changeSet) {
                $scope.changes = changeSet.changes;
                $scope.votes = changeSet.votes;
            });
        }])
        .controller('LoginCtrl', ['$scope', '$http', '$location', '$rootScope', 'AuthenticationService', function ($scope, $http, $location, $rootScope, authService) {
            $rootScope.hideHeader = true;
            
            $scope.login = function(user) {
                $http.post('/Security/Login', { emailAddress: user.username, password: user.password })
                    .success(function() {
                        authService.login(user);

                        var redirectUrl = $location.search().redirectUrl;
                        
                        if (redirectUrl) {
                            $location.url(redirectUrl);
                        }
                    }).error(function() {
                        //TODO: implement error handling
                    });
            };
        }])
        .controller('InitCtrl', ['UserConfig', 'AuthenticationService', function(userconfig, authService) {
            if (userconfig.authenticated) {
                authService.login(userconfig.user);
            }
        }]);
})();