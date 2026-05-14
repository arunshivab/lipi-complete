// SPEC:  docs/00-Phase2.6.2-Overview.md — Shared infrastructure + Ghost click prevention
// PHASE: 2.6.2 — Overlay Surfaces
// Usage: window.lipiOverlay.{activateFocusTrap, deactivateFocusTrap,
//                             lockBodyScroll, unlockBodyScroll}

window.lipiOverlay = (function () {
    'use strict';

    // ── Focus trap stack ───────────────────────────────────────────────────
    // Each entry: { container: Element, lastFocused: Element, keyHandler: fn }
    const _trapStack = [];

    // ── Ghost click guard ──────────────────────────────────────────────────
    // Tracks when an overlay is animating closed so late pointer events are
    // swallowed before they land on underlying controls.
    let _closingUntil = 0;  // timestamp (ms) until which clicks are ignored

    function _swallowGhostClick(e) {
        if (Date.now() < _closingUntil) {
            e.stopPropagation();
            e.preventDefault();
        }
    }

    function _armGhostGuard(durationMs) {
        _closingUntil = Date.now() + durationMs;
        // One-shot capture listener to eat the ghost click
        document.addEventListener('pointerdown', _swallowGhostClick, { capture: true, once: true });
        document.addEventListener('click',       _swallowGhostClick, { capture: true, once: true });
    }

    // ── Focusable selector ─────────────────────────────────────────────────
    function _getFocusable(container) {
        return Array.from(container.querySelectorAll(
            'a[href]:not([disabled]), ' +
            'button:not([disabled]):not([tabindex="-1"]), ' +
            'input:not([disabled]):not([type="hidden"]):not([tabindex="-1"]), ' +
            'select:not([disabled]):not([tabindex="-1"]), ' +
            'textarea:not([disabled]):not([tabindex="-1"]), ' +
            '[tabindex]:not([tabindex="-1"])'
        )).filter(el => !el.closest('[inert]') && !el.closest('[aria-hidden="true"]'));
    }

    // ── Focus trap activation ──────────────────────────────────────────────
    function activateFocusTrap(container, initialFocus) {
        if (!container) return;

        const trap = {
            container:   container,
            lastFocused: document.activeElement
        };

        // Move focus into the overlay
        const target = (initialFocus && typeof initialFocus.focus === 'function')
            ? initialFocus
            : (_getFocusable(container)[0] || container);

        // Defer focus move by one tick — avoids race with Blazor render cycle
        setTimeout(() => { if (target) target.focus(); }, 0);

        // Tab / Shift+Tab cycling scoped to topmost trap
        trap.keyHandler = function (e) {
            if (e.key !== 'Tab') return;
            const top = _trapStack[_trapStack.length - 1];
            if (!top || top !== trap) return;   // only topmost trap handles

            const focusable = _getFocusable(top.container);
            if (focusable.length === 0) { e.preventDefault(); return; }

            const first = focusable[0];
            const last  = focusable[focusable.length - 1];

            if (e.shiftKey && document.activeElement === first) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        };

        document.addEventListener('keydown', trap.keyHandler, true);

        // Backdrop captures mousedown to prevent click-through — ghost click rule
        // Backdrop element itself calls stopPropagation in its onclick handler.
        // This JS layer is the secondary guard.

        _trapStack.push(trap);
    }

    // ── Focus trap deactivation ────────────────────────────────────────────
    function deactivateFocusTrap() {
        const trap = _trapStack.pop();
        if (!trap) return;

        document.removeEventListener('keydown', trap.keyHandler, true);

        // Return focus to previous element — deferred by one tick so Blazor
        // DOM updates complete before focus moves (prevents scroll jump).
        setTimeout(() => {
            try {
                if (trap.lastFocused && typeof trap.lastFocused.focus === 'function') {
                    trap.lastFocused.focus({ preventScroll: true });
                }
            } catch (_) { /* element may have been removed from DOM */ }
        }, 0);

        // Arm ghost-click guard for 50ms so any late click from the closing
        // gesture doesn't land on whatever was under the overlay.
        _armGhostGuard(50);
    }

    // ── Body scroll lock ───────────────────────────────────────────────────
    // Compensates scrollbar width to prevent page shift when overflow:hidden
    // removes the vertical scrollbar. Stores scroll position for iOS Safari.
    let _scrollY = 0;
    let _scrollbarCompensated = false;

    function _getScrollbarWidth() {
        // Real measured scrollbar width — accounts for OS/browser differences
        return window.innerWidth - document.documentElement.clientWidth;
    }

    function lockBodyScroll() {
        _scrollY = window.scrollY;
        const sbWidth = _getScrollbarWidth();

        // Compensate for scrollbar disappearance — keep page width constant
        if (sbWidth > 0) {
            document.body.style.paddingRight = sbWidth + 'px';
            _scrollbarCompensated = true;
        }

        document.body.style.overflow = 'hidden';
        // iOS Safari needs position:fixed to prevent scroll-behind-modal
        document.body.style.position = 'fixed';
        document.body.style.top      = `-${_scrollY}px`;
        document.body.style.width    = '100%';
    }

    function unlockBodyScroll() {
        document.body.style.overflow      = '';
        document.body.style.position      = '';
        document.body.style.top           = '';
        document.body.style.width         = '';
        if (_scrollbarCompensated) {
            document.body.style.paddingRight = '';
            _scrollbarCompensated = false;
        }
        window.scrollTo(0, _scrollY);
    }

    // ── inert helper ───────────────────────────────────────────────────────
    // Sets inert on all page content outside the overlay (for modal-level overlays).
    // Targets: tn-content (main content area in TopNavLayout).
    function setPageInert(inert) {
        const contentEl = document.querySelector('.tn-content');
        const dockEl    = document.querySelector('.tn-dock');
        if (contentEl) contentEl.inert = inert;
        if (dockEl)    dockEl.inert    = inert;
    }

    // ── bfcache reset ──────────────────────────────────────────────────────
    // When browser restores page from back/forward cache, the Blazor scoped
    // CSS bundle may not be reattached. Force a full reload so all CSS links
    // are re-evaluated. event.persisted = true means bfcache restore.
    window.addEventListener('pageshow', function (e) {
        if (e.persisted) {
            window.location.reload();
        }
    });

    // ── DotNet host registration ───────────────────────────────────────────
    // LipiOverlayHost registers itself on first render so JS can call back
    // into Blazor for Escape handling — document-level listener fires
    // regardless of where focus currently is.
    let _dotNetHost = null;

    function registerHost(dotNetRef) {
        _dotNetHost = dotNetRef;
    }

    // Document-level Escape listener — fires even when focus is outside modal
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        if (_trapStack.length === 0 && !_dotNetHost) return;
        e.preventDefault();
        if (_dotNetHost) {
            _dotNetHost.invokeMethodAsync('OnEscapeKeyAsync').catch(function () {});
        }
    }, { capture: true });

    // ── Public API ─────────────────────────────────────────────────────────
    return {
        activateFocusTrap:   activateFocusTrap,
        deactivateFocusTrap: deactivateFocusTrap,
        lockBodyScroll:      lockBodyScroll,
        unlockBodyScroll:    unlockBodyScroll,
        setPageInert:        setPageInert,
        armGhostGuard:       _armGhostGuard,
        registerHost:        registerHost
    };
}());


// =============================================================================
// SPEC:  docs/03-LipiDynamicTabs-Spec.md §8 (amended A34)
// AMEND: docs/CHANGE-LOG.md A34
// PHASE: 2.6.2 — Overlay Surfaces
//
// window.lipiDtabs — chevron-driven overflow for LipiDynamicTabs.
//
// Public API:
//   lipiDtabs.attach(stripEl)              — wire observers + listeners
//   lipiDtabs.detach(stripEl)              — disconnect (call from Dispose)
//   lipiDtabs.startScroll(stripEl, dir)    — pointerdown handler ('left' | 'right')
//   lipiDtabs.stopScroll(stripEl)          — pointerup/leave/cancel handler
//
// Design (A34 build-chat decisions):
//   1. Visibility: hidden when no overflow, shown when overflow exists.
//      Driven by data-overflow="true|false" on the wrapper (.lipi-dtabs-
//      strip-wrapper). CSS reads this attr and sets chevron width 0 ↔ 32px.
//   2. At-edge state: data-can-scroll-left / data-can-scroll-right on the
//      wrapper, set from the strip's scrollLeft/scrollWidth/clientWidth.
//      Chevron `aria-disabled` mirrors this; CSS greys out the disabled one.
//   3. Click scroll: scrollBy(clientWidth - tabWidth, smooth). Tab width
//      measured from first .lipi-dtab child's offsetWidth (or 160px fallback).
//   4. Hold-to-scroll: first tick immediate. After 400ms delay, setInterval
//      120ms scrolls one tab width per tick. After 1s of holding, interval
//      drops to 60ms (acceleration). Pointerup/leave/cancel clears all timers.
//      If scroll reaches the boundary mid-hold, timer auto-stops.
//
// Per-strip state is held in a WeakMap keyed by the strip element so multiple
// strips on the same page don't share state.
// =============================================================================

window.lipiDtabs = (function () {
    'use strict';

    // Per-strip state: { wrapper, resizeObs, mutObs, scrollHandler,
    //                    holdTimer, repeatTimer, accelTimer, direction }
    const _state = new WeakMap();

    // ── Tunables ──────────────────────────────────────────────────────────
    const HOLD_DELAY_MS    = 400;   // pointerdown → first repeat tick
    const REPEAT_NORMAL_MS = 120;   // repeat interval (first 1s of holding)
    const REPEAT_FAST_MS   = 60;    // repeat interval after acceleration kicks in
    const ACCEL_AFTER_MS   = 1000;  // ms of holding before acceleration
    const FALLBACK_TAB_PX  = 160;   // used when no tabs yet measured

    // Tolerance for "is at edge" (avoids fractional-pixel false-negatives on
    // scrollLeft + clientWidth comparisons after smooth-scroll settles).
    const EDGE_EPSILON = 1;

    function _wrapperFor(stripEl) {
        // The wrapper is the immediate parent of the strip per A34 markup.
        return stripEl?.parentElement?.classList?.contains('lipi-dtabs-strip-wrapper')
            ? stripEl.parentElement
            : null;
    }

    function _measureTabWidth(stripEl) {
        const first = stripEl.querySelector('.lipi-dtab');
        return (first && first.offsetWidth > 0) ? first.offsetWidth : FALLBACK_TAB_PX;
    }

    function _updateOverflowState(stripEl) {
        const wrapper = _wrapperFor(stripEl);
        if (!wrapper) return;

        const hasOverflow = stripEl.scrollWidth > stripEl.clientWidth + EDGE_EPSILON;
        const canLeft     = stripEl.scrollLeft > EDGE_EPSILON;
        const canRight    = stripEl.scrollLeft + stripEl.clientWidth
                            < stripEl.scrollWidth - EDGE_EPSILON;

        wrapper.setAttribute('data-overflow',         hasOverflow ? 'true' : 'false');
        wrapper.setAttribute('data-can-scroll-left',  canLeft     ? 'true' : 'false');
        wrapper.setAttribute('data-can-scroll-right', canRight    ? 'true' : 'false');

        // Mirror onto chevron aria-disabled (CSS uses both attr + :disabled
        // selector, so this also greys them out visually).
        const leftBtn  = wrapper.querySelector('.lipi-dtabs-chevron-left');
        const rightBtn = wrapper.querySelector('.lipi-dtabs-chevron-right');
        if (leftBtn) {
            leftBtn.setAttribute('aria-disabled',  canLeft ? 'false' : 'true');
            if (canLeft) leftBtn.removeAttribute('disabled');
            else         leftBtn.setAttribute('disabled', '');
        }
        if (rightBtn) {
            rightBtn.setAttribute('aria-disabled', canRight ? 'false' : 'true');
            if (canRight) rightBtn.removeAttribute('disabled');
            else          rightBtn.setAttribute('disabled', '');
        }
    }

    function _scrollByPage(stripEl, dir) {
        const tabW   = _measureTabWidth(stripEl);
        const pageW  = Math.max(stripEl.clientWidth - tabW, tabW);
        const delta  = dir === 'left' ? -pageW : pageW;
        stripEl.scrollBy({ left: delta, behavior: 'smooth' });
    }

    function _scrollByTab(stripEl, dir) {
        const tabW  = _measureTabWidth(stripEl);
        const delta = dir === 'left' ? -tabW : tabW;
        stripEl.scrollBy({ left: delta, behavior: 'smooth' });
    }

    function _atEdge(stripEl, dir) {
        if (dir === 'left') {
            return stripEl.scrollLeft <= EDGE_EPSILON;
        }
        return stripEl.scrollLeft + stripEl.clientWidth
             >= stripEl.scrollWidth - EDGE_EPSILON;
    }

    function attach(stripEl) {
        if (!stripEl || _state.has(stripEl)) return;

        const wrapper = _wrapperFor(stripEl);
        if (!wrapper) return;

        // Initial measurement deferred one frame so layout is settled.
        requestAnimationFrame(function () { _updateOverflowState(stripEl); });

        // Resize observer → window resize, container resize, font load, etc.
        let resizeObs = null;
        if (typeof ResizeObserver !== 'undefined') {
            resizeObs = new ResizeObserver(function () {
                _updateOverflowState(stripEl);
            });
            resizeObs.observe(stripEl);
        }
        // Fallback: window resize listener for older browsers.
        const winResize = function () { _updateOverflowState(stripEl); };
        window.addEventListener('resize', winResize);

        // Mutation observer → tabs added/removed/title-changed.
        let mutObs = null;
        if (typeof MutationObserver !== 'undefined') {
            mutObs = new MutationObserver(function () {
                _updateOverflowState(stripEl);
            });
            mutObs.observe(stripEl, {
                childList: true,
                subtree:   true,
                attributes: true,
                attributeFilter: ['class', 'title']
            });
        }

        // Scroll listener → at-edge state updates as the strip is scrolled
        // (chevron click, smooth-scroll settle, programmatic scrollIntoView).
        const scrollHandler = function () { _updateOverflowState(stripEl); };
        stripEl.addEventListener('scroll', scrollHandler, { passive: true });

        _state.set(stripEl, {
            wrapper:       wrapper,
            resizeObs:     resizeObs,
            mutObs:        mutObs,
            winResize:     winResize,
            scrollHandler: scrollHandler,
            holdTimer:     null,
            repeatTimer:   null,
            accelTimer:    null,
            direction:     null
        });
    }

    function detach(stripEl) {
        const s = _state.get(stripEl);
        if (!s) return;

        if (s.resizeObs) {
            try { s.resizeObs.disconnect(); } catch (e) { /* ignore */ }
        }
        if (s.mutObs) {
            try { s.mutObs.disconnect(); } catch (e) { /* ignore */ }
        }
        if (s.winResize) {
            window.removeEventListener('resize', s.winResize);
        }
        if (s.scrollHandler) {
            try { stripEl.removeEventListener('scroll', s.scrollHandler); }
            catch (e) { /* ignore */ }
        }
        _clearTimers(s);
        _state.delete(stripEl);
    }

    function _clearTimers(s) {
        if (s.holdTimer)   { clearTimeout(s.holdTimer);   s.holdTimer   = null; }
        if (s.repeatTimer) { clearInterval(s.repeatTimer); s.repeatTimer = null; }
        if (s.accelTimer)  { clearTimeout(s.accelTimer);  s.accelTimer  = null; }
        s.direction = null;
    }

    function startScroll(stripEl, direction) {
        const s = _state.get(stripEl);
        if (!s) return;
        if (direction !== 'left' && direction !== 'right') return;
        if (_atEdge(stripEl, direction)) return; // greyed-out chevron — no-op

        _clearTimers(s);
        s.direction = direction;

        // Immediate first scroll (click case + start of hold).
        _scrollByPage(stripEl, direction);

        // After HOLD_DELAY_MS of sustained press, start repeating at NORMAL rate.
        s.holdTimer = setTimeout(function () {
            s.repeatTimer = setInterval(function () {
                if (_atEdge(stripEl, s.direction)) {
                    _clearTimers(s);
                    return;
                }
                _scrollByTab(stripEl, s.direction);
            }, REPEAT_NORMAL_MS);

            // After ACCEL_AFTER_MS more, swap to FAST rate.
            s.accelTimer = setTimeout(function () {
                if (!s.repeatTimer) return;
                clearInterval(s.repeatTimer);
                s.repeatTimer = setInterval(function () {
                    if (_atEdge(stripEl, s.direction)) {
                        _clearTimers(s);
                        return;
                    }
                    _scrollByTab(stripEl, s.direction);
                }, REPEAT_FAST_MS);
            }, ACCEL_AFTER_MS);
        }, HOLD_DELAY_MS);
    }

    function stopScroll(stripEl) {
        const s = _state.get(stripEl);
        if (!s) return;
        _clearTimers(s);
    }

    // ── Public API ─────────────────────────────────────────────────────────
    return {
        attach:      attach,
        detach:      detach,
        startScroll: startScroll,
        stopScroll:  stopScroll
    };
}());
