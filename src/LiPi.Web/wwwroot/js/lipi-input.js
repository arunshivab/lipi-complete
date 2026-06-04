// SPEC:  Phase 2.2 / 2.3 — JS helpers for LiPi input + compound component families.
// USE:   Loaded via App.razor as a global script; exposes window.lipiInput +
//        window.lipiCompound + window.lipiDatePicker.
// SCOPE: autogrow (Batch 3) + selectAll (Batch 4) + setValue (Batch 4.3) +
//        dropdown positioning / outside-click / scroll-reposition (Batch 5) +
//        compound-field focusout listener with relatedTarget contains-check (Batch 9a) +
//        scrollOptionIntoView for keyboard-navigated dropdown highlight (Batch 9b.3).
// AMEND: docs/CHANGE-LOG.md A14 (Batches 4–5), A19 (Batch 9a/9b, pending).
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
    // SCROLL HIGHLIGHTED OPTION INTO VIEW — Batch 9b.3 (Phase 2.3)
    // ─────────────────────────────────────────────────────────────────────────
    //
    // After keyboard navigation (Arrow keys / type-ahead) changes
    // _highlightedIndex on the C# side, the matching option DOM element may
    // be outside the dropdown panel's visible scroll region. Without scrolling
    // it into view, the user sees no visual feedback even though the highlight
    // moved.
    //
    // The C# template renders each option with data-option-index="N" matching
    // the global highlight index. This helper finds that element by selector
    // and calls scrollIntoView({block:'nearest'}) — which scrolls only as much
    // as needed to make the element visible (no jarring viewport jumps).
    //
    // 'nearest' is preferred over 'center' or 'start' because:
    //   - 'nearest': scrolls the minimum needed; if already visible, does nothing
    //   - 'center': always centers, causes jumpy feel even for adjacent items
    //   - 'start': scrolls highlighted item to top of panel, hides items above

    const scrollOptionIntoView = (panelId, index) => {
        if (!panelId || index < 0) return;
        try {
            const panel = document.getElementById(panelId);
            if (!panel) return;
            const option = panel.querySelector(`[data-option-index="${index}"]`);
            if (!option) return;
            option.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        } catch (e) {
            // Selector or scrollIntoView may fail in older browsers or unusual
            // DOM states. Silent fail — keyboard navigation still updates the
            // highlight; user can scroll manually.
        }
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
        detachSelectHandlers:   detachSelectHandlers,
        scrollOptionIntoView:   scrollOptionIntoView
    };

    // ─────────────────────────────────────────────────────────────────────────
    // COMPOUND FIELD HELPERS — Batch 9a (Phase 2.3)
    // ─────────────────────────────────────────────────────────────────────────
    //
    // Owns the focusout listener for LipiCompoundField. C# cannot do this
    // natively because Blazor's FocusEventArgs does NOT surface RelatedTarget
    // (that's a DOM web-API property, not exposed in the .NET event args).
    //
    // Strategy: native focusout listener on the wrapper element. On each
    // focusout event, check whether event.relatedTarget is still a descendant
    // of the wrapper. If it IS, focus moved between segments — do nothing
    // (mid-interaction). If it ISN'T, focus actually left the compound — call
    // back into C# via dotNetRef.invokeMethodAsync('OnFocusLeftCompound').
    //
    // State stored on the wrapper element itself via _lipiCompoundFocusOutHandler
    // so detach can find and remove the listener cleanly. Idempotent —
    // attachFocusOut on an already-attached element is a no-op.

    const attachFocusOut = (wrapper, dotNetRef) => {
        if (!wrapper || !dotNetRef) return;
        if (wrapper._lipiCompoundFocusOutHandler) return; // already attached

        const handler = (e) => {
            // e.relatedTarget is the element receiving focus. If null (e.g.,
            // focus moved to the document body via blur), treat as "left."
            // If it's a descendant of wrapper, focus stayed inside — ignore.
            if (e.relatedTarget && wrapper.contains(e.relatedTarget)) return;

            // Real focus-out: tell C# to mark touched + aggregate validation.
            dotNetRef.invokeMethodAsync('OnFocusLeftCompound').catch(() => {
                // Circuit may have closed; swallow silently. The C# side
                // will get re-attached on next render if circuit reconnects.
            });
        };

        wrapper.addEventListener('focusout', handler);
        wrapper._lipiCompoundFocusOutHandler = handler;
    };

    const detachFocusOut = (wrapper) => {
        if (!wrapper || !wrapper._lipiCompoundFocusOutHandler) return;
        wrapper.removeEventListener('focusout', wrapper._lipiCompoundFocusOutHandler);
        delete wrapper._lipiCompoundFocusOutHandler;
    };

    window.lipiCompound = {
        attachFocusOut: attachFocusOut,
        detachFocusOut: detachFocusOut
    };

    // ─────────────────────────────────────────────────────────────────────────
    // DATE PICKER POPOVER POSITIONING — Batch 9d (Phase 2.4)
    // ─────────────────────────────────────────────────────────────────────────
    //
    // Position strategy: position: fixed, JS-calculated viewport coordinates.
    // (Corrected from original Phase 2.4 design's portal-via-JS approach;
    // see CHANGE-LOG A20 for rationale.)
    //
    // Why position:fixed not absolute:
    //   - position:fixed is relative to viewport, NOT to nearest positioned
    //     ancestor. This means popover escapes overflow:hidden on parent
    //     wrappers (modals, dialogs, scrollable side panels) without needing
    //     to be detached from its DOM location. Same pattern as LipiSelect's
    //     dropdown panel (Phase 2.2 Batch 5).
    //   - One known gotcha: ancestors with `transform`, `filter`, or
    //     `will-change` properties create a new containing block that DOES
    //     constrain position:fixed. LiPi components don't currently use
    //     these, but if they're introduced (e.g., for animation), document
    //     and revisit.
    //
    // Why JS for the math (not pure CSS):
    //   - Edge-aware flip up/down based on available viewport space requires
    //     reading getBoundingClientRect() of the anchor — purely CSS can't
    //     do this. CSS @media queries operate on viewport, not on per-anchor
    //     position.
    //   - Reposition on scroll/resize keeps popover anchored as user scrolls
    //     the page underneath. Native DOM events; no Blazor SignalR roundtrip.
    //
    // Mobile fallback:
    //   - Below 640px viewport, CSS @media query overrides position:fixed
    //     to a full-overlay modal style. No JS change needed; positionPopover
    //     still runs but the CSS overrides take precedence.

    const positionPopover = (anchorEl, popoverEl) => {
        if (!anchorEl || !popoverEl) return;

        const rect = anchorEl.getBoundingClientRect();
        const popHeight = popoverEl.offsetHeight;
        const popWidth = popoverEl.offsetWidth;
        const viewportH = window.innerHeight;
        const viewportW = window.innerWidth;

        // Vertical: prefer below anchor. Flip up if not enough space below
        // AND there IS enough space above. The 16px buffer matches typical
        // form-bar margins so popover never visually touches anchor edges.
        const spaceBelow = viewportH - rect.bottom;
        const spaceAbove = rect.top;
        const flipUp = spaceBelow < popHeight + 16 && spaceAbove > popHeight + 16;

        // Horizontal: anchor left edge is the default. If popover would
        // overflow viewport right edge, shift left by the overflow amount + 8px buffer.
        // If popover is wider than viewport (extreme case), align left at 8px from edge.
        let leftPx;
        if (popWidth >= viewportW - 16) {
            leftPx = 8;
        } else {
            const overflowRight = (rect.left + popWidth) - viewportW;
            leftPx = overflowRight > 0
                ? Math.max(8, rect.left - overflowRight - 8)
                : rect.left;
        }

        popoverEl.style.position   = 'fixed';
        popoverEl.style.left       = `${leftPx}px`;
        popoverEl.style.top        = flipUp
            ? `${rect.top - popHeight - 8}px`
            : `${rect.bottom + 8}px`;
        popoverEl.style.visibility = 'visible';
    };

    // Attach scroll + resize listeners that call positionPopover whenever the
    // page geometry changes. Returns a detach function so C# can clean up
    // when the popover closes.
    //
    // We pass useCapture=true on the scroll listener so we catch scroll events
    // on ANY ancestor (not just window) — the user may be scrolling inside a
    // modal body or a scrollable side panel, and we want the popover to track.
    //
    // The repositioning is throttled via requestAnimationFrame — without this,
    // every scroll pixel triggers a layout read + write, which is laggy.
    const attachReposition = (anchorEl, popoverEl) => {
        if (!anchorEl || !popoverEl) return null;

        let rafId = 0;
        const reposition = () => {
            if (rafId) cancelAnimationFrame(rafId);
            rafId = requestAnimationFrame(() => {
                positionPopover(anchorEl, popoverEl);
                rafId = 0;
            });
        };

        window.addEventListener('scroll', reposition, true);  // capture phase
        window.addEventListener('resize', reposition);

        // Return detach function — C# stores this and calls on popover close.
        // JUDGMENT: returning a function is JS-idiomatic but Blazor JS interop
        // doesn't round-trip functions cleanly. We work around this by attaching
        // the detach function to the popover element itself, and providing a
        // separate detachReposition helper that finds and calls it. Callers
        // that don't need explicit detach can ignore the return.
        const detach = () => {
            if (rafId) cancelAnimationFrame(rafId);
            window.removeEventListener('scroll', reposition, true);
            window.removeEventListener('resize', reposition);
        };

        // Stash on element so detachReposition can find it
        popoverEl._lipiDatePickerDetach = detach;

        return null;  // don't attempt to round-trip the function across interop
    };

    const detachReposition = (popoverEl) => {
        if (!popoverEl || !popoverEl._lipiDatePickerDetach) return;
        popoverEl._lipiDatePickerDetach();
        delete popoverEl._lipiDatePickerDetach;
    };

    // Focus a specific element. Used by date pickers for:
    //   - Auto-focus first segment when popover opens
    //   - Auto-focus next segment after auto-advance
    //   - Auto-focus time field after date picked (LipiDateTimePicker D2.1)
    // Defensive against null + non-focusable elements.
    const focusElement = (el) => {
        if (!el || typeof el.focus !== 'function') return;
        try {
            el.focus();
            // For text inputs, also select-all so user can re-type immediately
            // without manually clearing. Matches Phase 2.2 LipiTextBox pattern.
            if (typeof el.select === 'function' &&
                (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA')) {
                // JUDGMENT: select-all on focus is intentional for date segments —
                // user typing replaces the existing value, which is what they want
                // when re-entering a segment. Matches lipi-input selectAll behavior.
                el.select();
            }
        } catch (_) {
            // Some elements throw on focus when in a weird state (detached
            // from DOM, hidden, etc.). Silently ignore — focus is a "best
            // effort" UX nicety, not a correctness requirement.
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // OUTSIDE-CLICK DISMISSAL — Issue 1 fix (May 8 patch)
    //
    // When the popover is open and user clicks anywhere outside the wrapper,
    // close the popover. Standard popover-library pattern.
    //
    // Why mousedown not click:
    //   - mousedown fires before click. By the time we get a click event,
    //     focus may have already shifted, validation may have run, etc.
    //     Catching the user's intent at mousedown gives the cleanest UX.
    //   - Mobile/touch: synthesized mousedown fires for taps too. No
    //     additional handler needed.
    //
    // Why we still skip when target is inside wrapper:
    //   - Calendar cells, year/month dropdowns, today button — all live
    //     inside the wrapper (or inside the popover, which is positioned
    //     by JS but not detached from DOM). Their own click handlers
    //     close the popover when appropriate. We don't want to interfere.
    //
    // Idempotent: attaching twice is a no-op.

    const attachOutsideClick = (wrapper, dotNetRef) => {
        if (!wrapper || !dotNetRef) return;
        if (wrapper._lipiDatePickerOutsideHandler) return;

        const handler = (e) => {
            // If click is inside the wrapper (or popover descendants),
            // ignore — let the inner click handlers do their thing.
            if (wrapper.contains(e.target)) return;
            // Outside click — tell C# to close the popover.
            dotNetRef.invokeMethodAsync('OnOutsideClick').catch(() => {
                // Circuit may have closed or been re-rendered; swallow.
            });
        };

        document.addEventListener('mousedown', handler, true);  // capture phase
        wrapper._lipiDatePickerOutsideHandler = handler;
    };

    const detachOutsideClick = (wrapper) => {
        if (!wrapper || !wrapper._lipiDatePickerOutsideHandler) return;
        document.removeEventListener('mousedown', wrapper._lipiDatePickerOutsideHandler, true);
        delete wrapper._lipiDatePickerOutsideHandler;
    };

    // ─────────────────────────────────────────────────────────────────────────
    // KEYBOARD-NAV PREVENT-DEFAULT — Issue 2 fix (May 8 patch)
    //
    // Calendar grid uses PageUp/PageDown/Home/End/Arrow keys for navigation.
    // Browser default is to scroll the page. We need to preventDefault on
    // these keys WHEN THE POPOVER IS THE ACTIVE FOCUS — not globally, since
    // those keys serve other purposes elsewhere on the page.
    //
    // Approach: attach a keydown listener to the popover element itself
    // (or its descendants via bubbling). On the navigation keys, call
    // preventDefault. Blazor's @onkeydown handler still fires because it's
    // wired to the same event — preventDefault stops the browser default
    // (scroll), not the event itself.
    //
    // Razor's :preventDefault modifier doesn't accept runtime expressions
    // for "preventDefault iff key matches a list", so JS interception is
    // the simplest path.

    const NAV_KEYS = new Set([
        'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight',
        'PageUp', 'PageDown',
        'Home', 'End',
        ' ',  // Space — browser default is scroll
    ]);

    const attachKeyboardTrap = (popoverEl) => {
        if (!popoverEl) return;
        if (popoverEl._lipiDatePickerKeyTrap) return;

        const handler = (e) => {
            if (NAV_KEYS.has(e.key)) {
                e.preventDefault();
                // Note: do NOT stopPropagation — Blazor's @onkeydown still
                // needs to fire to update _highlightedDate state.
            }
        };

        popoverEl.addEventListener('keydown', handler);
        popoverEl._lipiDatePickerKeyTrap = handler;
    };

    const detachKeyboardTrap = (popoverEl) => {
        if (!popoverEl || !popoverEl._lipiDatePickerKeyTrap) return;
        popoverEl.removeEventListener('keydown', popoverEl._lipiDatePickerKeyTrap);
        delete popoverEl._lipiDatePickerKeyTrap;
    };

    window.lipiDatePicker = {
        positionPopover:    positionPopover,
        attachReposition:   attachReposition,
        detachReposition:   detachReposition,
        focusElement:       focusElement,
        attachOutsideClick: attachOutsideClick,
        detachOutsideClick: detachOutsideClick,
        attachKeyboardTrap: attachKeyboardTrap,
        detachKeyboardTrap: detachKeyboardTrap
    };
})();

// ── DateTime migration (A54): client time-source bridge ──────────────────────
// When a picker's TimeSource = Client, it reads the BROWSER's local clock/zone here
// (the server cannot know the client's local date). Returns the local wall-clock parts
// + the tz offset (minutes, JS sign convention: behind-UTC is positive). The picker
// builds a local DateTime/DateOnly from the parts (no instant math needed for Today/Now).
(function () {
    if (!window.lipiInput) { window.lipiInput = {}; }
    window.lipiInput.getClientNow = function () {
        var d = new Date();
        return {
            iso: d.toISOString(),
            tzOffsetMin: d.getTimezoneOffset(),
            year: d.getFullYear(),
            month: d.getMonth() + 1,   // JS months are 0-based
            day: d.getDate(),
            hour: d.getHours(),
            minute: d.getMinutes(),
            second: d.getSeconds()
        };
    };
})();
