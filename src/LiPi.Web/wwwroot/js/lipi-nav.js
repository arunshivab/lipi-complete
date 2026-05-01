// LiPi Navigation JS — FAB lifecycle only.
// Keyboard handling moved to lipi-shortcuts.js (single source of truth).
// PatientFab.razor calls lipiNav.initFab / lipiNav.disposeFab on render and
// dispose; this file keeps that contract intact while doing nothing harmful.
(function () {
    'use strict';

    window.lipiNav = {
        // Called from PatientFab.razor OnAfterRenderAsync.
        // Stores the DotNetObjectReference for any future component-level use.
        initFab: function (dotNetRef) {
            window.__lipiNavRef = dotNetRef;
        },

        // Called from PatientFab.razor DisposeAsync.
        disposeFab: function () {
            window.__lipiNavRef = null;
        }
    };
}());
