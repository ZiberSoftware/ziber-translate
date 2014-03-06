(function () {
    'use strict';

    var app = angular.module('TranslateApp', ['ngRoute', 'ngSanitize', 'pasvaz.bindonce', 'Translate.Controllers', 'Translate.Directives']);

    app.config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
        $routeProvider
            .when('/sets/:id/translations/:language/:page', {
                templateUrl: '/Scripts/partials/Translation.html'
            })
            .when('/sets', {
                templateUrl: '/Scripts/partials/Sets.html'
            })
            .when('/login', {
                templateUrl: '/Scripts/partials/Login.html'
            })
            .when('/changeset', {
                templateUrl: '/Scripts/partials/Changeset.html'
            })
            .otherwise({
                redirectTo: '/sets'
            });

        $httpProvider.interceptors.push('mvcHttpInterceptor');
    }]);
})();