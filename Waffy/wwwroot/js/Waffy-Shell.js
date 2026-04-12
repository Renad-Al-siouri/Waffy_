/* ══════════════════════════════════════════════════════
   WAFFY SUPER ADMIN — UNIFIED SHELL SCRIPT
   Handles sidebar, topnav, accordion, mobile
   Include ONCE before </body> on every page
══════════════════════════════════════════════════════ */
(function () {
    const shell = document.getElementById('waShell');
    const sb = document.getElementById('waSb');
    const ovl = document.getElementById('waOvl');
    const mob = () => window.innerWidth <= 992;
    let mini = false;

    function save(v) { localStorage.setItem('wa_mini', v); }
    function miniOn() { mini = true; shell.classList.add('mini'); save('1'); collapseAllSubsForMini(); }
    function miniOff() { mini = false; shell.classList.remove('mini'); save('0'); restoreOpenAccordion(); }
    function openMob() { sb.classList.add('mob-open'); ovl.classList.add('show'); document.body.style.overflow = 'hidden'; }
    function closeMob() { sb.classList.remove('mob-open'); ovl.classList.remove('show'); document.body.style.overflow = ''; }

    window.toggleSb = function () {
        if (mob()) { sb.classList.contains('mob-open') ? closeMob() : openMob(); }
        else { mini ? miniOff() : miniOn(); }
    };
    window.closeMob = closeMob;

    function getSubInnerH(el) { const i = el.querySelector('.sb-sub-inner'); return i ? i.scrollHeight : 0; }
    function openSub(g, t) { g.classList.add('open'); t.classList.add('open'); g.style.height = getSubInnerH(g) + 'px'; }
    function closeSub(g, t) { g.classList.remove('open'); t.classList.remove('open'); g.style.height = '0px'; }

    function closeAllSubs() {
        document.querySelectorAll('.sb-sub').forEach(s => { s.classList.remove('open'); s.style.height = '0px'; });
        document.querySelectorAll('[id^="t-"]').forEach(t => t.classList.remove('open'));
    }
    function collapseAllSubsForMini() {
        document.querySelectorAll('.sb-sub').forEach(s => { s.style.height = '0px'; });
    }
    function restoreOpenAccordion() {
        document.querySelectorAll('.sb-sub.open').forEach(s => { s.style.height = getSubInnerH(s) + 'px'; });
    }

    window.acc = function (gId, tId) {
        const g = document.getElementById(gId);
        const t = document.getElementById(tId);
        if (!g || !t) return;
        const isOpen = g.classList.contains('open');
        if (!mob() && mini) { miniOff(); setTimeout(() => handleAccordion(g, t, isOpen), 180); return; }
        handleAccordion(g, t, isOpen);
    };

    function handleAccordion(g, t, isOpen) { closeAllSubs(); if (!isOpen) openSub(g, t); }

    /* Mark active link based on current page */
    function markActive() {
        const page = location.pathname.split('/').pop();
        if (!page) return;
        document.querySelectorAll('.sb-child, .sb-row[href]').forEach(el => {
            const h = el.getAttribute('href') || '';
            if (h && page === h) {
                el.classList.add('active');
                const sub = el.closest('.sb-sub');
                if (sub) {
                    sub.classList.add('open');
                    const trigger = document.getElementById(sub.id.replace('g-', 't-'));
                    if (trigger) trigger.classList.add('open');
                }
            }
        });
        restoreOpenAccordion();
    }

    /* Set topnav / hero text */
    window.setPageInfo = function (title, sub, section, page) {
        const el = id => document.getElementById(id);
        if (el('tnTitle')) el('tnTitle').textContent = title || '';
        if (el('tnSub')) el('tnSub').textContent = sub || '';
        if (el('tnCur')) el('tnCur').textContent = page || '';
        if (section && el('tnBread'))
            el('tnBread').innerHTML = `<span>${section}</span><span>/</span><b id="tnCur">${page || ''}</b>`;
        if (el('pageHeroTitle')) el('pageHeroTitle').textContent = title || '';
    };

    /* Resize handler */
    let rt;
    window.addEventListener('resize', () => {
        clearTimeout(rt);
        rt = setTimeout(() => {
            if (!mob()) {
                closeMob();
                localStorage.getItem('wa_mini') === '1' ? miniOn() : miniOff();
                restoreOpenAccordion();
            } else {
                shell.classList.remove('mini'); mini = false;
                document.querySelectorAll('.sb-sub.open').forEach(s => { s.style.height = getSubInnerH(s) + 'px'; });
            }
        }, 90);
    });

    /* Keyboard shortcuts */
    document.addEventListener('keydown', e => {
        if (e.key === '[' && !e.ctrlKey && !e.metaKey) window.toggleSb();
        if (e.key === 'Escape') closeMob();
    });

    /* Touch swipe */
    let tx = 0;
    document.addEventListener('touchstart', e => { tx = e.touches[0].clientX; }, { passive: true });
    document.addEventListener('touchend', e => {
        const dx = e.changedTouches[0].clientX - tx;
        if (!mob()) return;
        if (dx > 55 && tx < 28) openMob();
        if (dx < -55) closeMob();
    }, { passive: true });

    /* Init */
    document.addEventListener('DOMContentLoaded', () => {
        if (!mob() && localStorage.getItem('wa_mini') === '1') miniOn();
        markActive();
        restoreOpenAccordion();
    });
})();