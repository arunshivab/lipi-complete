// =============================================================================
// SPEC:  docs/00-COMPONENTS/2.7/04-LipiValidationSummary-Spec.md §6
// PHASE: 2.7 — Feedback Components
// AMEND: docs/CHANGE-LOG.md A35 (2026-05-15)
//
// JS interop for LipiValidationSummary click-to-field behavior. Exposes a
// single namespace `window.lipiValidation` with `scrollToField(fieldId)`.
//
// Loaded via App.razor (Phase 2.7 batch). Failure modes:
//   - Field id not found → silent no-op (consumer page may have changed; the
//     summary itself doesn't break, just the navigation effect)
//   - scrollIntoView unsupported (very old browser) → fallback to focus only
//   - focus() fails (element hidden) → silent
// =============================================================================

(function (window) {
    'use strict';

    if (!window.lipiValidation) {
        window.lipiValidation = {};
    }

    /**
     * Scroll a form field into view, focus it, and briefly highlight with a
     * flash ring. Called from LipiValidationSummary when the user clicks an
     * error item in the bulleted list.
     *
     * @param {string} fieldId — HTML id of the target input/select/textarea.
     */
    window.lipiValidation.scrollToField = function (fieldId) {
        if (!fieldId) return;

        var el = document.getElementById(fieldId);
        if (!el) {
            // The field may not be in the DOM (conditional rendering, virtualized
            // list, hidden tab). Silent no-op — error stays visible in summary.
            return;
        }

        // Smooth scroll the field into the middle of the viewport. Falls back
        // to an instant jump on browsers without smooth-scroll support — both
        // bring the field on-screen, which is the essential effect.
        try {
            if (typeof el.scrollIntoView === 'function') {
                el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        } catch (e) {
            // Some browsers throw on options object — try the legacy form.
            try { el.scrollIntoView(); } catch (e2) { /* give up — focus only */ }
        }

        // Defer focus so the scroll animation doesn't get interrupted by the
        // focus-induced scroll. 300ms aligns with typical smooth-scroll duration.
        setTimeout(function () {
            try {
                el.focus({ preventScroll: true });
            } catch (e) {
                // preventScroll option missing on older browsers — fall back.
                try { el.focus(); } catch (e2) { /* element not focusable */ }
            }
        }, 300);

        // Flash ring. Add class, schedule removal after the keyframe completes
        // (1.5s in lipi-validation.css). Removing the class is important so the
        // animation can replay if the same field is clicked again.
        try {
            el.classList.add('lipi-field-flash');
            setTimeout(function () {
                el.classList.remove('lipi-field-flash');
            }, 1500);
        } catch (e) {
            // classList absent (truly ancient browser) — animation is a polish
            // effect, scroll+focus already happened, no further action needed.
        }
    };
})(window);
