function byId(id) {
    return document.getElementById(id);
}

const SW_STEPS = [
    { label: 'Store Owner Information', sub: 'Step 1', ico: '👤' },
    { label: 'Store Information', sub: 'Step 2', ico: '🏪' }
];

let swCur = 1;

// -----------------------------------------------
// Sidebar + Progress
// -----------------------------------------------
function swRenderSidebar() {
    const panel = byId('swSbSteps');
    if (!panel) return;

    let h = '';
    SW_STEPS.forEach((s, i) => {
        const n = i + 1;
        const cls = n < swCur ? 'done' : n === swCur ? 'active' : '';
        const num = n < swCur ? '✓' : n;
        h += `
            <div class="sw-si ${cls}">
                <div class="sw-si-num">${num}</div>
                <div>
                    <div class="sw-si-title">${s.ico} ${s.label}</div>
                    <div class="sw-si-sub">${s.sub}</div>
                </div>
            </div>`;
    });
    panel.innerHTML = h;
}

function swUpdateProgress() {
    const fill = document.querySelector('.prog-fill');
    if (!fill) return;
    fill.style.width = ({ 1: '50%', 2: '100%' })[swCur] || '50%';
}

// -----------------------------------------------
// Validation helpers
// -----------------------------------------------
function swCe(el) {
    if (!el) return;
    el.classList.remove('err');
    const errId = el.id ? ('sw-e-' + el.id.replace('sw-', '')) : '';
    const e = byId(errId);
    if (e) e.classList.remove('show');
}

function swReq(id) {
    const el = byId(id);
    if (!el) return false;
    if (!String(el.value || '').trim()) {
        el.classList.add('err');
        const e = byId('sw-e-' + id.replace('sw-', ''));
        if (e) e.classList.add('show');
        return false;
    }
    return true;
}

function swVEmail(id) {
    const el = byId(id);
    if (!el) return true;
    const ok = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(el.value);
    if (!ok) {
        el.classList.add('err');
        const e = byId('sw-e-' + id.replace('sw-', ''));
        if (e) e.classList.add('show');
    }
    return ok;
}

function swPwdStr(inp) {
    if (!inp) return;
    const v = inp.value;
    let s = 0;
    if (v.length >= 8) s++;
    if (/[0-9]/.test(v)) s++;
    if (/[A-Z]/.test(v) || /[!@#$%^&*]/.test(v)) s++;

    document.querySelectorAll('#sw-ps .sw-pb').forEach((b, i) => {
        b.className = 'sw-pb' + (i < s ? ' ' + ['w', 'm', 's'][s - 1] : '');
    });

    const pl = byId('sw-pl');
    if (pl) pl.textContent = s > 0 ? ['Weak', 'Medium', 'Strong'][s - 1] : 'Enter a password';
}

// -----------------------------------------------
// ✅ Step 1 — validation قبل submit الـ Form
//    الـ Form يرسل نفسه بشكل عادي (POST)
//    هذه الدالة تُستدعى من onsubmit
// -----------------------------------------------
function swValidateStep1() {
    const ok = swReq('sw-fn') & swReq('sw-ln') & swVEmail('sw-oemail') & swReq('sw-ophone');

    const p1 = byId('sw-opwd') ? byId('sw-opwd').value : '';
    const p2 = byId('sw-opwd2') ? byId('sw-opwd2').value : '';

    if (!ok) return false;

    if (p1.length < 8) {
        byId('sw-opwd').classList.add('err');
        return false;
    }

    if (p1 !== p2) {
        byId('sw-opwd2').classList.add('err');
        byId('sw-e-opwd2').classList.add('show');
        return false;
    }

    return true; // يسمح للـ form بالإرسال
}

// -----------------------------------------------
// ✅ Step 2 — validation قبل submit الـ Form
// -----------------------------------------------
function swValidateStep2() {
    const ok = swReq('sw-sname') & swVEmail('sw-semail') & swReq('sw-sphone') & swReq('sw-scity');
    if (!ok) return false;

    const doc1 = byId('sw-doc1');
    const doc2 = byId('sw-doc2');

    if (!doc1 || !doc1.files.length) {
        alert('⚠ Please upload the Commercial Registration document.');
        return false;
    }
    if (!doc2 || !doc2.files.length) {
        alert('⚠ Please upload the Business License document.');
        return false;
    }

    return true; // يسمح للـ form بالإرسال
}

// -----------------------------------------------
// File upload helpers
// -----------------------------------------------
function swTrigUpload(id) {
    const el = byId(id);
    if (el) el.click();
}

function swPreviewFile(inp, prevId, nameId) {
    if (!inp || !inp.files || !inp.files[0]) return;
    const n = byId(nameId);
    if (n) n.textContent = inp.files[0].name;
    const p = byId(prevId);
    if (p) p.classList.add('show');
}

function swRemoveFile(fileId, prevId, nameId) {
    const f = byId(fileId);
    if (f) f.value = '';
    const n = byId(nameId);
    if (n) n.textContent = '';
    const p = byId(prevId);
    if (p) p.classList.remove('show');
}

// -----------------------------------------------
// Init
// -----------------------------------------------
document.addEventListener('DOMContentLoaded', () => {
    swCur = document.body.classList.contains('store-step-2') ? 2 : 1;
    swRenderSidebar();
    swUpdateProgress();
});