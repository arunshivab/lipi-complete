// LiPi Top-Nav clock — runs once, ticks every second
(function () {
    'use strict';
    function pad(n) { return n < 10 ? '0' + n : '' + n; }
    function tick() {
        var el = document.getElementById('tn-clock-time');
        var dt = document.getElementById('tn-clock-date');
        if (!el) return;
        var now = new Date();
        el.textContent = pad(now.getHours()) + ':' + pad(now.getMinutes()) + ':' + pad(now.getSeconds());
        if (dt) {
            dt.textContent = now.toLocaleDateString(undefined,
                { weekday: 'short', day: '2-digit', month: 'short', year: 'numeric' });
        }
    }
    if (!window.__tnClockStarted) {
        window.__tnClockStarted = true;
        tick();
        setInterval(tick, 1000);
    } else {
        tick();
    }
})();

// ── LiPi Form helpers ─────────────────────────────────────────────────────
window.lipiForm = {
    focusFirstError: function () {
        // Find the topmost visible input/select that has the err class
        var el = document.querySelector('.uf-inp.err, .uf-sel.err');
        if (el) {
            el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            el.focus();
        }
    }
};
