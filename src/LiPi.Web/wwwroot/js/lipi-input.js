// SPEC:  Phase 2.2 — JS helpers for the LiPi input component family.
// USE:   Loaded via App.razor as a global script; exposes window.lipiInput.
// SCOPE: autogrow (Batch 3) + selectAll (Batch 4) + setValue (Batch 4.3) +
//        dropdown positioning / outside-click / scroll-reposition (Batch 5).
// AMEND: docs/CHANGE-LOG.md A14 covers Batches 4 / 4.1 / 4.2 / 4.3 / 5 as one
//        Phase 2.2 sub-step entry (pending).
//
// Why this approach (not field-sizing CSS):
//   The CSS field-sizing: content property (Chrome 123+, Safari 17.4+, Firefox flagged)
//   would obviate this whole file. But HIS deployments include older clinical workstations
//   running pinned Chrome/Edge for IT compliance — we can't rely on it for v1.0.
//   Revisit in v1.1 when browser baseline is set.
//
// Why native event listener (not Blazor @oninput round-trip):
//   On Blazor Server (the LiPi deployment model), every keystroke roundtripped through
//   SignalR adds 5-15ms of perceptible latency on top of typing. Attaching the autogrow
//   handler as a native DOM event listener keeps it on the client side — zero server hop.
//   Blazor still receives oninput in parallel for value binding; both fire from the same
//   browser event with no ordering dependency between them.

(function () {
    'use strict';

    // ─────────────────────────────────────────────────────────────────────────
    // AUTOGROW (LipiTextArea) — Batch 3
    // ─────────────────────────────────────────────────────────────────────────

    // Compute the height ceiling for a given textarea + maxRows. Reads computed line-height,
    // padding, and border so the calculation tracks any CSS changes (different size variants,
    // theme changes, font swaps).
    const computeMaxHeight = (textarea, maxRows) => {
        const cs = getComputedStyle(textarea);
        const lineHeight    = parseFloat(cs.lineHeight)       || 20;
        const paddingTop    = parseFloat(cs.paddingTop)       || 0;
        const paddingBottom = parseFloat(cs.paddingBottom)    || 0;
        const borderTop     = parseFloat(cs.borderTopWidth)   || 0;
        const borderBottom  = parseFloat(cs.borderBottomWidth) || 0;
        return Math.ceil(lineHeight * maxRows + paddingTop + paddingBottom + borderTop + borderBottom);
    };

    // One-shot measure-and-resize. Idempotent — safe to call repeatedly.
    const autogrow = (textarea, maxRows) => {
        if (!textarea) return;

        // Reset to 'auto' first so scrollHeight reflects the natural content height,
        // not the previously-set explicit height. Without this, the textarea can never shrink.
        textarea.style.height = 'auto';

        const maxHeight = computeMaxHeight(textarea, maxRows);
        const newHeight = Math.min(textarea.scrollHeight, maxHeight);

        textarea.style.height    = newHeight + 'px';
        textarea.style.overflowY = textarea.scrollHeight > maxHeight ? 'auto' : 'hidden';
    };

    // Attach a native input listener for live autogrow. Idempotent — no-op if already attached.
    // Stores the handler on the element so detachAutogrow can find and remove it.
    const attachAutogrow = (textarea, maxRows) => {
        if (!textarea) return;
        if (textarea._lipiAutogrowHandler) return; // already attached — protect against double-attach during re-render

        const handler = () => autogrow(textarea, maxRows);
        textarea.addEventListener('input', handler);
        textarea._lipiAutogrowHandler = handler;
        textarea._lipiAutogrowMaxRows = maxRows;

        // Initial measure — handles textareas with multi-line initial values loaded from DB.
        handler();
    };

    // Cleanup attached listener. Called from C# Dispose. Defensive — handles already-detached
    // and never-attached elements without throwing.
    const detachAutogrow = (textarea) => {
        if (!textarea || !textarea._lipiAutogrowHandler) return;
        textarea.removeEventListener('input', textarea._lipiAutogrowHandler);
        delete textarea._lipiAutogrowHandler;
        delete textarea._lipiAutogrowMaxRows;
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SELECT-ALL ON FOCUS (LipiNumberInput) — Batch 4
    // ─────────────────────────────────────────────────────────────────────────
    //
    // Called from LipiNumberInput's HandleFocus. The component has just re-rendered with
    // raw display (e.g., "1,23,456" → "123456"). Selecting all means the user's first
    // keystroke replaces the value entirely — standard spreadsheet / finance-app UX.
    //
    // Why JS not autofocus + native select(): Blazor's render cycle places the value
    // update slightly after the focus event reaches us. Calling select() from C# before
    // the new raw value lands selects the OLD formatted text. Doing it from JS after
    // the render completes selects the new raw text correctly.

    const selectAll = (input) => {
        if (!input || typeof input.select !== 'function') return;
        // requestAnimationFrame defers until after the current render commits, so we
        // select the freshly-painted raw value, not the formatted value that was there
        // a microsecond ago.
        requestAnimationFrame(() => {
            try { input.select(); } catch { /* element may have been detached */ }
        });
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SET VALUE WITH CURSOR PRESERVATION (LipiNumberInput) — Batch 4.3
    // ─────────────────────────────────────────────────────────────────────────
    //
    // Called from LipiNumberInput's HandleInput when the BlockNonNumericInput filter has
    // stripped invalid characters but Blazor's render diff won't push the corrected value
    // to the DOM (because the filtered string happens to match the prior render's value
    // attribute — the value-sync edge case documented in Batch 4.2 ship notes).
    //
    // Cursor handling: if the user typed an invalid character at position N and the filter
    // stripped characters before that position, the cursor needs to shift back accordingly.
    // Simple heuristic: cursor goes to min(prior cursor position, new value length). Works
    // correctly for the common cases (typing invalid char at end, pasting mixed content,
    // typing minus mid-string). Edge case: typing invalid char in the middle of a longer
    // valid string puts cursor slightly off, but that's better than jumping to start or end.

    const setValue = (input, value) => {
        if (!input) return;
        // No-op if already correct — avoids unnecessary cursor manipulation.
        if (input.value === value) return;

        const prevCursor = input.selectionStart || 0;
        input.value = value;

        // Restore cursor position. Some input types (e.g., type=email, type=number) don't
        // support setSelectionRange and throw — wrap in try/catch.
        const newCursor = Math.min(prevCursor, value.length);
        try {
            input.setSelectionRange(newCursor, newCursor);
        } catch {
            // Ignore — cursor will land at default position (browser-specific).
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SELECT / COMBOBOX DROPDOWN HELPERS — Batch 5
    // ─────────────────────────────────────────────────────────────────────────
    //
    // Three responsibilities:
    //   1. Position the fixed-positioned dropdown panel relative to its anchor input.
    //      Includes flip-up logic when there's not enough space below the anchor.
    //   2. Close the dropdown on outside-click (anywhere outside anchor or dropdown).
    //   3. Reposition (NOT close) on viewport/ancestor scroll. Closing on scroll is
    //      jarring for HIS contexts (clinical workstations may have tracking pointers,
    //      accessibility scroll modes). Material UI / Carbon both reposition; we match.
    //
    // State stored on window._lipiSelectState by dropdown id, so multiple selects can
    // coexist on the same page without leaking listeners. attachSelectHandlers is
    // idempotent — calling it twice for the same id replaces the prior state.

    if (!window._lipiSelectState) {
        window._lipiSelectState = {};
    }

    // Compute and apply top/left styles on the fixed-positioned dropdown. Flips above
    // the anchor when there's insufficient space below.
    const positionDropdown = (anchor, dropdownId) => {
        if (!anchor) return;
        const dropdown = document.getElementById(dropdownId);
        if (!dropdown) return;

        const rect = anchor.getBoundingClientRect();
        const viewportH = window.innerHeight;
        const viewportW = window.innerWidth;
        const gap = 4; // small visual gap between anchor and dropdown

        // Force a layout read of the dropdown to know its actual height (after content render)
        // Use getBoundingClientRect rather than offsetHeight to handle transforms.
        const ddRect = dropdown.getBoundingClientRect();
        const ddHeight = ddRect.height || 320; // fallback to max-height if not yet laid out
        const ddWidth = Math.max(ddRect.width, rect.width); // dropdown at least as wide as anchor

        // Vertical: prefer below; flip above if insufficient space below AND more space above
        const spaceBelow = viewportH - rect.bottom;
        const spaceAbove = rect.top;
        let top;
        if (spaceBelow >= ddHeight + gap || spaceBelow >= spaceAbove) {
            top = rect.bottom + gap;
        } else {
            top = rect.top - ddHeight - gap;
        }

        // Horizontal: align with anchor's left edge; clamp to viewport
        let left = rect.left;
        if (left + ddWidth > viewportW - 8) {
            left = Math.max(8, viewportW - ddWidth - 8);
        }
        if (left < 8) left = 8;

        dropdown.style.top = top + 'px';
        dropdown.style.left = left + 'px';
        dropdown.style.minWidth = rect.width + 'px';
    };

    // Wire up outside-click + scroll listeners. Calls back into Blazor via DotNetObjectReference
    // when the user clicks outside or scrolls an ancestor.
    const attachSelectHandlers = (anchor, dotNetRef, dropdownId) => {
        if (!anchor || !dotNetRef || !dropdownId) return;

        // Defensive cleanup for re-attachment
        detachSelectHandlers(dropdownId);

        const onDocClick = (e) => {
            const dropdown = document.getElementById(dropdownId);
            // Click is "outside" when not within anchor AND not within dropdown
            if (anchor.contains(e.target)) return;
            if (dropdown && dropdown.contains(e.target)) return;
            dotNetRef.invokeMethodAsync('OnOutsideClick').catch(() => {});
        };

        const onScroll = () => {
            // Reposition on any scroll event (capture phase catches all scrollable ancestors).
            // Throttle via requestAnimationFrame to avoid flooding on momentum scrolls.
            if (window._lipiSelectState[dropdownId]?.rafId) return;
            const rafId = requestAnimationFrame(() => {
                positionDropdown(anchor, dropdownId);
                dotNetRef.invokeMethodAsync('OnAncestorScroll').catch(() => {});
                if (window._lipiSelectState[dropdownId]) {
                    window._lipiSelectState[dropdownId].rafId = null;
                }
            });
            if (window._lipiSelectState[dropdownId]) {
                window._lipiSelectState[dropdownId].rafId = rafId;
            }
        };

        const onResize = () => {
            positionDropdown(anchor, dropdownId);
        };

        // Use capture: true to catch scroll events on scrollable ancestors
        // (scroll events don't bubble, but they do propagate during capture phase).
        document.addEventListener('mousedown', onDocClick, true);
        window.addEventListener('scroll', onScroll, true);
        window.addEventListener('resize', onResize, false);

        window._lipiSelectState[dropdownId] = {
            anchor: anchor,
            onDocClick: onDocClick,
            onScroll: onScroll,
            onResize: onResize,
            rafId: null
        };
    };

    const detachSelectHandlers = (dropdownId) => {
        const state = window._lipiSelectState[dropdownId];
        if (!state) return;
        document.removeEventListener('mousedown', state.onDocClick, true);
        window.removeEventListener('scroll', state.onScroll, true);
        window.removeEventListener('resize', state.onResize, false);
        if (state.rafId) cancelAnimationFrame(state.rafId);
        delete window._lipiSelectState[dropdownId];
    };

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────────

    window.lipiInput = {
        autogrow:               autogrow,
        attachAutogrow:         attachAutogrow,
        detachAutogrow:         detachAutogrow,
        selectAll:              selectAll,
        setValue:               setValue,
        positionDropdown:       positionDropdown,
        attachSelectHandlers:   attachSelectHandlers,
        detachSelectHandlers:   detachSelectHandlers
    };
})();
