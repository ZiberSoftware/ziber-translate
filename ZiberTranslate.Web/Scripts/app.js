(function () {
    'use strict';

    var app = angular.module('TranslateApp', ['ngRoute', 'ngSanitize', 'pasvaz.bindonce', 'Translate.Controllers', 'Translate.Directives']);

    app.config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
        $routeProvider
            .when('/sets/:id/translations/:language/:filter/:page', {
                templateUrl: '/Scripts/partials/Translation.html'
            })
            .when('/sets', {
                templateUrl: '/Scripts/partials/Sets.html'
            })
            .when('/changeset', {
                templateUrl: '/Scripts/partials/Changeset.html'
            })
            .when('/admin', {
                templateUrl: '/Scripts/partials/AdminSets.html'
            })
            .when('/admin/:id/review-translations/:language', {
                templateUrl: '/Scripts/partials/AdminTranslations.html'
            })
            .otherwise({
                redirectTo: '/sets'
            });

        $httpProvider.interceptors.push('mvcHttpInterceptor');
    }]);
})();