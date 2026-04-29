(function () {
    'use strict';

    var machines = [
        { name: 'CT Scanner',         dept: 'Computed Tomography',       code: 'CT' },
        { name: 'MRI',                dept: 'Magnetic Resonance',         code: 'MRI' },
        { name: 'Linac',              dept: 'Radiation Therapy',          code: 'LINAC' },
        { name: 'PET-CT',             dept: 'Nuclear Medicine',           code: 'PETCT' },
        { name: 'Cath Lab',           dept: 'Interventional Cardiology',  code: 'CATH' },
        { name: 'Operation Theatre',  dept: 'Surgical Suite',             code: 'OT' },
        { name: 'Brachytherapy',      dept: 'Radiation Oncology',         code: 'BRACHY' }
    ];

    var current = 0;

    function cycle() {
        var scenes = document.querySelectorAll('.machine-scene-slot');
        var ticks  = document.querySelectorAll('.ticker-mark');
        var dept   = document.getElementById('machine-dept');
        var name   = document.getElementById('machine-name');
        var label  = document.querySelector('.machine-label-code');

        // Hide all scenes
        for (var i = 0; i < scenes.length; i++) {
            scenes[i].style.opacity = '0';
            scenes[i].style.pointerEvents = 'none';
        }

        // Advance index
        current = (current + 1) % machines.length;

        // Fade in new scene
        if (scenes[current]) {
            scenes[current].style.opacity = '1';
            scenes[current].style.pointerEvents = '';
        }

        // Update label text
        if (dept) dept.textContent = machines[current].dept;
        if (name) name.textContent = machines[current].name;
        if (label) label.textContent = machines[current].code;

        // Update ticker dots
        for (var j = 0; j < ticks.length; j++) {
            ticks[j].classList.toggle('active', j === current);
            var span = ticks[j].querySelector('.ticker-label');
            if (span) span.textContent = j === current ? machines[j].code : '';
        }
    }

    function init() {
        // Ensure first machine is visible
        var scenes = document.querySelectorAll('.machine-scene-slot');
        for (var i = 0; i < scenes.length; i++) {
            scenes[i].style.transition = 'opacity 0.6s ease';
            scenes[i].style.opacity = (i === 0) ? '1' : '0';
            scenes[i].style.pointerEvents = (i === 0) ? '' : 'none';
        }

        // Start cycling every 1 second
        setInterval(cycle, 3000);
    }

    // Captcha refresh — generate a random 6-char code client-side
    window.refreshLoginCaptcha = function () {
        var chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
        var code = '';
        for (var k = 0; k < 6; k++) {
            code += chars[Math.floor(Math.random() * chars.length)];
        }
        var display = document.getElementById('captcha-display');
        var input   = document.getElementById('captcha-input');
        var hidden  = document.getElementById('captcha-value');
        if (display) display.textContent = code;
        if (input)   input.value = '';
        if (hidden)  hidden.value = code;
    };

    // OTP toggle — switch button label
    window.handleOTPToggle = function (checkbox) {
        var btn = document.getElementById('submit-btn');
        if (btn) btn.textContent = checkbox.checked ? 'Send OTP \u2192' : 'Sign In';
    };

    // Run after DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
