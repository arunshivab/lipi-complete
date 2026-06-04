// lipi-table.js — LipiTable component-local JS interop
// PHASE: 2.8 Data Display — Stage 3 (rows/cells: copy affordance)
//
// Ships inside the LiPi.Components package (wwwroot) so the redistributable table is
// self-contained — no dependency on a host clipboard helper. Loaded via App.razor as
// _content/LiPi.Components/lipi-table.js (Blazor static web asset path).
//
// Single function: copy text to the clipboard. Returns true on success, false on failure.
// The component handles the "Copied!" flash + optional OnCopy callback on the .NET side;
// JS only performs the clipboard write. Uses the async Clipboard API where available,
// with a execCommand fallback for older/non-secure contexts.

window.lipiTable = window.lipiTable || {};

window.lipiTable.copy = async function (text) {
    if (text == null) return false;
    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(String(text));
            return true;
        }
    } catch (e) {
        // fall through to the legacy path
    }

    // Legacy fallback: hidden textarea + execCommand('copy').
    try {
        const ta = document.createElement("textarea");
        ta.value = String(text);
        ta.setAttribute("readonly", "");
        ta.style.position = "absolute";
        ta.style.left = "-9999px";
        document.body.appendChild(ta);
        ta.select();
        const ok = document.execCommand("copy");
        document.body.removeChild(ta);
        return ok;
    } catch (e) {
        return false;
    }
};

// ── Stage S3a: popover anchor — return an element's viewport rect so a position:fixed
// popover can escape the table's overflow:hidden and never clip. The caller passes the
// funnel button's ElementReference; we return {top,left,bottom,right,width} in px.
window.lipiTable.getRect = function (el) {
    if (!el || typeof el.getBoundingClientRect !== "function") return null;
    const r = el.getBoundingClientRect();
    return { top: r.top, left: r.left, bottom: r.bottom, right: r.right, width: r.width, height: r.height,
             viewportH: window.innerHeight, viewportW: window.innerWidth };
};

// ── Stage S3a: close the filter popover on any scroll (capture phase catches scrolls in
// any ancestor/container, not just window). Registered when a popover opens; the .NET side
// passes a DotNetObjectReference whose ClosePopoverFromJs() is invoked once, then we detach.
window.lipiTable.onScrollClose = function (dotnetRef) {
    // Detach any prior handlers first (idempotent).
    window.lipiTable.offScrollClose();

    const close = function () {
        window.lipiTable.offScrollClose();
        try { dotnetRef.invokeMethodAsync("ClosePopoverFromJs"); } catch (e) {}
    };
    // Scroll (capture: catches nested scrollers too) dismisses the popover.
    const scrollH = function () { close(); };
    // Esc dismisses the popover regardless of where focus sits.
    const keyH = function (e) {
        if (e.key === "Escape" || e.key === "Esc") { e.stopPropagation(); close(); }
    };
    window.lipiTable._scrollHandler = scrollH;
    window.lipiTable._popoverKeyHandler = keyH;
    document.addEventListener("scroll", scrollH, true);
    document.addEventListener("keydown", keyH, true);
};
window.lipiTable.offScrollClose = function () {
    if (window.lipiTable._scrollHandler) {
        document.removeEventListener("scroll", window.lipiTable._scrollHandler, true);
        window.lipiTable._scrollHandler = null;
    }
    if (window.lipiTable._popoverKeyHandler) {
        document.removeEventListener("keydown", window.lipiTable._popoverKeyHandler, true);
        window.lipiTable._popoverKeyHandler = null;
    }
};

// ── Stage 4d (A48): keyboard scroll-guard ───────────────────────────────────
// Blazor Server can't conditionally preventDefault per-key (the browser decides before
// the .NET handler runs). So a single delegated capture-phase listener suppresses the
// browser's default page-scroll for the grid navigation/selection keys — but ONLY when
// focus is inside a LipiTable body cell, and NEVER for Tab/Escape (those must pass through
// so keyboard focus can leave the grid). All actual navigation/selection logic stays in
// the component's @onkeydown (.NET) — this guard only stops the scroll.
window.lipiTable.initKeyboardGuard = function () {
    if (window.lipiTable._kbGuardInstalled) return;   // idempotent
    window.lipiTable._kbGuardInstalled = true;

    const SUPPRESS = new Set([
        "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight",
        "Home", "End", " ", "Spacebar"
    ]);

    document.addEventListener("keydown", function (e) {
        // Only inside a focusable LipiTable body cell.
        const target = e.target;
        if (!target || typeof target.closest !== "function") return;
        const cell = target.closest(".lipi-table-cell, .lipi-table-cell-select");
        if (!cell) return;

        // Don't fight typing inside an input/textarea (e.g., future inline edit).
        const tag = (target.tagName || "").toLowerCase();
        if (tag === "input" || tag === "textarea" || target.isContentEditable) return;

        // PageUp/PageDown only conflict with the browser scroll for our Alt+Page chord; leave
        // plain PageUp/PageDown alone so a user can still scroll a focused table normally.
        if ((e.key === "PageUp" || e.key === "PageDown")) {
            if (e.altKey) e.preventDefault();
            return;
        }
        if (SUPPRESS.has(e.key)) {
            e.preventDefault();    // stop page scroll; .NET @onkeydown still runs the logic
        }
        // Tab, Escape, and everything else pass through untouched.
    }, true);   // capture phase — before the browser scrolls
};
