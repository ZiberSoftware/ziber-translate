(function() {
    'use strict';

    angular.module('Translate.Services', []);
    angular.module('Translate.Controllers', ['Translate.Services', 'pasvaz.bindonce']);
})();