/**
 * LiPi HIS — theme-switcher.js
 * SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Theme Switching Mechanism
 * DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
 * Phase:    Phase 1 — Theming Architecture, Deliverable 6
 *
 * PURPOSE:
 *   1. FOUC prevention — auto-initializes immediately when parsed (before first paint).
 *      Reads lipi_theme_mode + lipi_brand_theme cookies and applies data attributes
 *      to <html> before any rendering occurs.
 *   2. API for Blazor interop — window.lipiTheme.apply(brand, mode) is called by
 *      ThemeProvider.razor via: JS.InvokeVoidAsync("lipiTheme.apply", brand, mode)
 *
 * LOAD POSITION:
 *   Blocking script in <head> — NO async or defer attributes.
 *   Position: BEFORE <HeadOutlet> and BEFORE blazor.web.js.
 *   This guarantees execution before the browser paints any content.
 *
 * TARGET ELEMENT:
 *   Applies data-brand and data-mode to document.documentElement (<html>).
 *   CSS selectors are unqualified [attr] — they match on <html> and custom
 *   properties cascade to all descendants including <body>.
 *   Using documentElement (not body) ensures the script works even when
 *   called from <head> before <body> exists in the DOM.
 *
 * COOKIE POLICY (mirrors IThemeContextService.ThemeCookieOptions):
 *   path=/, max-age=31536000 (1 year), samesite=strict, secure (HTTPS only)
 *   HttpOnly=false intentional — JS must read cookies for FOUC prevention.
 */

(function () {
    'use strict';

    // ── Cookie helpers ────────────────────────────────────────────────────────

    /**
     * Read a cookie by name. Returns null if not found.
     * Handles URL-encoded values. Regex-escapes name to prevent injection.
     */
    function readCookie(name) {
        var escaped = name.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        var match   = document.cookie.match(
            new RegExp('(?:^|; )' + escaped + '=([^;]*)'));
        return match ? decodeURIComponent(match[1]) : null;
    }

    /**
     * Write a theme cookie matching IThemeContextService.ThemeCookieOptions.
     * Adds 'secure' flag only on HTTPS — local dev (http) works without it.
     */
    function writeCookie(name, value) {
        var parts = [
            name + '=' + encodeURIComponent(value),
            'path=/',
            'max-age=31536000',
            'samesite=strict'
        ];
        if (location.protocol === 'https:') {
            parts.push('secure');
        }
        document.cookie = parts.join('; ');
    }

    // ── Constants ─────────────────────────────────────────────────────────────

    // VALID_MODES must stay in sync with ThemeContextService.ValidModes (C# HashSet).
    // Brand validation is intentionally omitted — brand values are server-authoritative
    // (come from master.brand_themes). We trust whatever is in the cookie.
    var VALID_MODES   = ['light', 'dark', 'auto', 'high-contrast'];
    var DEFAULT_MODE  = 'light';
    var DEFAULT_BRAND = 'lipi-default';

    // Cookie names must match IThemeContextService constants exactly.
    var COOKIE_MODE  = 'lipi_theme_mode';
    var COOKIE_BRAND = 'lipi_brand_theme';

    // ── Validation helpers ────────────────────────────────────────────────────

    function safeMode(raw) {
        return (raw && VALID_MODES.indexOf(raw) !== -1) ? raw : DEFAULT_MODE;
    }

    function safeBrand(raw) {
        return (raw && typeof raw === 'string' && raw.trim().length > 0)
            ? raw.trim()
            : DEFAULT_BRAND;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /**
     * Apply brand + mode to the document.
     * Sets data-brand and data-mode on <html> (documentElement).
     * Updates cookies so the next SSR request skips the DB lookup.
     *
     * Called by:
     *   - init() on page load (FOUC prevention)
     *   - ThemeProvider.razor: JS.InvokeVoidAsync("lipiTheme.apply", brand, mode)
     *
     * @param {string} brand  Brand identifier (e.g. "lipi-default", "armoki")
     * @param {string} mode   Mode identifier (e.g. "light", "dark")
     */
    function apply(brand, mode) {
        brand = safeBrand(brand);
        mode  = safeMode(mode);

        document.documentElement.setAttribute('data-brand', brand);
        document.documentElement.setAttribute('data-mode',  mode);

        writeCookie(COOKIE_MODE,  mode);
        writeCookie(COOKIE_BRAND, brand);
    }

    /**
     * Read the current theme from document attributes.
     * Reflects what is currently active — may differ from cookies if
     * ThemeProvider hasn't synced yet (e.g., on first circuit connect).
     *
     * @returns {{ brand: string, mode: string }}
     */
    function getCurrentTheme() {
        return {
            brand: document.documentElement.getAttribute('data-brand') || DEFAULT_BRAND,
            mode:  document.documentElement.getAttribute('data-mode')  || DEFAULT_MODE
        };
    }

    /**
     * Initialize theme from cookies.
     * Safe to call multiple times — idempotent.
     * ThemeProvider.OnAfterRenderAsync calls lipiTheme.apply() directly
     * (not init()) to avoid a redundant cookie read after the DB-authoritative
     * value is already known.
     */
    function init() {
        apply(
            readCookie(COOKIE_BRAND) || DEFAULT_BRAND,
            readCookie(COOKIE_MODE)  || DEFAULT_MODE
        );
    }

    // ── Expose public API ─────────────────────────────────────────────────────

    window.lipiTheme = {
        apply:           apply,
        getCurrentTheme: getCurrentTheme,
        init:            init
    };

    // ── Auto-initialize ───────────────────────────────────────────────────────
    // Executes IMMEDIATELY when script is parsed — before browser paints content.
    // This is the FOUC prevention mechanism. The blocking <script> in <head>
    // guarantees execution order: cookies read → attributes set → CSS applied
    // → browser first paint. No flash of wrong theme.
    init();

}());
