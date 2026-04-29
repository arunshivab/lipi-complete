// LiPi Theme Manager
// Applies data-theme="dark" or "light" to <html> element.
// Called from Blazor via IJSRuntime.
(function () {
    'use strict';

    window.lipiTheme = {

        // Call on app init — applies saved theme immediately to avoid flash
        init: function () {
            var saved = localStorage.getItem('lipi-theme') || 'light';
            document.documentElement.setAttribute('data-theme', saved);
            return saved;
        },

        // Set and persist a theme
        set: function (theme) {
            localStorage.setItem('lipi-theme', theme);
            document.documentElement.setAttribute('data-theme', theme);
        },

        // Get current theme
        get: function () {
            return localStorage.getItem('lipi-theme') || 'light';
        }
    };

    // Apply immediately on script load to prevent flash of wrong theme
    window.lipiTheme.init();

})();
