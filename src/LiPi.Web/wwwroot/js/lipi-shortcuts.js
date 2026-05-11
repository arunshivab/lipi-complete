// LiPi Keyboard Shortcuts
// Loaded via App.razor <script src="js/lipi-shortcuts.js">
//
// SAFE BY DESIGN — no Ctrl/Cmd/Alt-modified shortcuts. None of the bindings
// here collide with reserved combos in Chrome, Edge, Safari, Firefox or Opera.
//
// Three layers:
//   1) F-keys for top actions:  F2 F4 F6 F7 F8 F9
//      (F1/F3/F5/F11/F12 are reserved by browsers and intentionally skipped)
//   2) G then a letter (leader-key sequence):  G F | G N | G A | G Q | G D
//      G E | G P | G S | G C
//   3) ?  shows the shortcut help
//
// Letter-based shortcuts are suppressed while focus is in INPUT, TEXTAREA,
// SELECT or contenteditable elements so users can type freely.
// F-keys fire regardless of focus (clinical-software muscle memory).

(function () {
    'use strict';

    // -----------------------------------------------------------------------
    // Action table — one source of truth for keys, routes and labels
    // -----------------------------------------------------------------------
    var ACTIONS = {
        find:       { url: '/patients/search',        label: 'Find patient' },
        register:   { url: '/patients/new',           label: 'Register patient' },
        appt:       { url: '/appointments/book',      label: 'Book appointment' },
        queue:      { url: '/patients/queue',         label: 'OPD queue' },
        dashboard:  { url: '/dashboard',              label: 'Dashboard' },
        emergency:  { url: null,                      label: 'Emergency registration', notBuilt: true },
        profile:    { url: '/profile',                label: 'My profile' },
        settings:   { url: '/admin',                  label: 'Settings · Admin' },
        calendar:   { url: '/appointments/calendar',  label: 'Calendar' }
    };

    var FN_MAP = {
        F2: 'find',
        F4: 'register',
        F6: 'appt',
        F7: 'queue',
        F8: 'dashboard',
        F9: 'emergency'
    };

    var G_MAP = {
        f: 'find',
        n: 'register',
        a: 'appt',
        q: 'queue',
        d: 'dashboard',
        e: 'emergency',
        p: 'profile',
        s: 'settings',
        c: 'calendar'
    };

    // -----------------------------------------------------------------------
    // G-leader state
    // -----------------------------------------------------------------------
    var gPending = false;
    var gTimer = null;
    var G_TIMEOUT_MS = 1200;

    function startG() {
        gPending = true;
        if (gTimer) clearTimeout(gTimer);
        gTimer = setTimeout(clearG, G_TIMEOUT_MS);
        showHint(true);
    }

    function clearG() {
        gPending = false;
        if (gTimer) { clearTimeout(gTimer); gTimer = null; }
        showHint(false);
    }

    // -----------------------------------------------------------------------
    // Tiny visual hint shown while waiting for the second key of a G-sequence
    // -----------------------------------------------------------------------
    function showHint(on) {
        var el = document.getElementById('lipi-shortcut-hint');
        if (on) {
            if (!el) {
                el = document.createElement('div');
                el.id = 'lipi-shortcut-hint';
                el.style.cssText =
                    'position:fixed;bottom:24px;left:50%;transform:translateX(-50%);' +
                    'background:#0B2545;color:#fff;padding:9px 16px;border-radius:6px;' +
                    'font-family:"LiPi Mono",ui-monospace,Menlo,monospace;font-size:12px;' +
                    'letter-spacing:0.5px;z-index:99999;box-shadow:0 6px 16px rgba(11,37,69,0.25);' +
                    'pointer-events:none;';
                document.body.appendChild(el);
            }
            el.textContent = 'G — F·N·A·Q·D·E·P·S·C   (Esc to cancel)';
        } else if (el) {
            el.parentNode.removeChild(el);
        }
    }

    // -----------------------------------------------------------------------
    // Toast — used for "module not yet built" and the help shortcut
    // -----------------------------------------------------------------------
    function toast(msg, kind) {
        var palette = kind === 'warn'
            ? { bg: '#FEF3C7', fg: '#92400E', bd: '#FCD34D' }
            : { bg: '#DCFCE7', fg: '#065F46', bd: '#6EE7B7' };
        var el = document.createElement('div');
        el.style.cssText =
            'position:fixed;bottom:24px;left:50%;transform:translateX(-50%);' +
            'background:' + palette.bg + ';color:' + palette.fg + ';' +
            'border:1px solid ' + palette.bd + ';padding:9px 16px;border-radius:6px;' +
            'font-family:"LiPi Sans",system-ui,sans-serif;font-size:13px;font-weight:500;' +
            'z-index:99999;box-shadow:0 6px 16px rgba(11,37,69,0.18);' +
            'transition:opacity 0.3s;';
        el.textContent = msg;
        document.body.appendChild(el);
        setTimeout(function () { el.style.opacity = '0'; }, 2200);
        setTimeout(function () { if (el.parentNode) el.parentNode.removeChild(el); }, 2600);
    }

    // -----------------------------------------------------------------------
    // Navigation — prefer Blazor's enhanced navigation, fall back to full load
    // -----------------------------------------------------------------------
    function navigate(url) {
        if (window.Blazor && typeof window.Blazor.navigateTo === 'function') {
            try { window.Blazor.navigateTo(url, false); return; }
            catch (e) { /* fall through */ }
        }
        window.location.href = url;
    }

    // -----------------------------------------------------------------------
    // Action dispatcher
    // -----------------------------------------------------------------------
    function fire(key) {
        var a = ACTIONS[key];
        if (!a) return;
        if (a.notBuilt) {
            toast(a.label + ' — not yet available', 'warn');
            return;
        }
        if (a.url) navigate(a.url);
    }

    // -----------------------------------------------------------------------
    // Help — placeholder until a proper overlay is designed
    // -----------------------------------------------------------------------
    function showHelp() {
        var lines = [
            '=== LiPi keyboard shortcuts ===',
            'F2  Find patient        F4  Register patient',
            'F6  Book appointment    F7  OPD queue',
            'F8  Dashboard           F9  Emergency  (not built yet)',
            '',
            'Or press G then a letter:',
            '  G F  Find          G N  Register      G A  Appointment',
            '  G Q  Queue         G D  Dashboard     G E  Emergency',
            '  G P  Profile       G S  Settings      G C  Calendar',
            '',
            'Press ? again to see this list.'
        ];
        if (window.console && console.info) console.info(lines.join('\n'));
        toast('Shortcut list logged to console (full overlay coming soon)', 'info');
    }

    // -----------------------------------------------------------------------
    // Focus filter — never fire letter/? shortcuts while user is typing
    // -----------------------------------------------------------------------
    function isTyping(e) {
        var t = e.target;
        if (!t) return false;
        var tag = (t.tagName || '').toUpperCase();
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return true;
        if (t.isContentEditable) return true;
        return false;
    }

    // -----------------------------------------------------------------------
    // Master keydown handler
    // -----------------------------------------------------------------------
    function onKey(e) {
        // Ctrl/Cmd/Alt are never used by LiPi — let the browser have them
        if (e.ctrlKey || e.metaKey || e.altKey) return;

        // Esc cancels a pending G-leader; otherwise let it bubble (modals, etc.)
        if (e.key === 'Escape') {
            if (gPending) { clearG(); e.preventDefault(); }
            return;
        }

        // Layer 1 — F-keys (fire even when typing; clinical muscle memory)
        if (FN_MAP[e.key]) {
            e.preventDefault();
            clearG();
            fire(FN_MAP[e.key]);
            return;
        }

        // Layers 2 & 3 — letter-based; skip while typing
        if (isTyping(e)) return;

        // Layer 3 — ? for help (Shift + /)
        if (e.key === '?') {
            e.preventDefault();
            clearG();
            showHelp();
            return;
        }

        var k = (e.key || '').toLowerCase();

        // Layer 2 — G-leader sequence
        if (gPending) {
            // Second key after G
            if (G_MAP[k]) {
                e.preventDefault();
                clearG();
                fire(G_MAP[k]);
            } else {
                clearG();
            }
            return;
        }

        // First key — was it G?
        if (k === 'g') {
            e.preventDefault();
            startG();
            return;
        }
    }

    // Idempotent registration — re-loading the script must not double-bind
    if (!window.__lipiShortcutsBound) {
        document.addEventListener('keydown', onKey, false);
        window.__lipiShortcutsBound = true;
        window.lipiShortcuts = {
            fire: fire,
            help: showHelp,
            actions: ACTIONS
        };
    }
}());
