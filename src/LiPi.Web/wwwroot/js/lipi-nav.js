// LiPi Navigation JS — Patient FAB keyboard shortcuts
// Loaded via App.razor <script src="js/lipi-nav.js">
(function () {
    'use strict';

    window.lipiNav = {

        // Called from PatientFab.razor OnAfterRenderAsync
        // Stores the DotNetObjectReference and registers global keydown handler
        initFab: function (dotNetRef) {
            window.__lipiNavRef = dotNetRef;

            // Register handler once; re-registering replaces the ref only
            if (!window.__lipiNavHandler) {
                window.__lipiNavHandler = function (e) {
                    if (!window.__lipiNavRef) return;
                    if (!(e.ctrlKey || e.metaKey)) return;

                    // Never fire inside form inputs
                    var tag = e.target ? e.target.tagName.toUpperCase() : '';
                    if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;

                    var k = e.key;
                    if (k === 'n' || k === 'N') {
                        e.preventDefault();
                        window.__lipiNavRef.invokeMethodAsync('HandleShortcut', 'new');
                    } else if (k === 'f' || k === 'F') {
                        e.preventDefault();
                        window.__lipiNavRef.invokeMethodAsync('HandleShortcut', 'search');
                    } else if (k === 'q' || k === 'Q') {
                        e.preventDefault();
                        window.__lipiNavRef.invokeMethodAsync('HandleShortcut', 'queue');
                    } else if (k === 'Escape') {
                        window.__lipiNavRef.invokeMethodAsync('HandleShortcut', 'close');
                    }
                };
                document.addEventListener('keydown', window.__lipiNavHandler);
            }
        },

        // Called from PatientFab.razor DisposeAsync
        disposeFab: function () {
            window.__lipiNavRef = null;
            if (window.__lipiNavHandler) {
                document.removeEventListener('keydown', window.__lipiNavHandler);
                window.__lipiNavHandler = null;
            }
        }
    };
}());
