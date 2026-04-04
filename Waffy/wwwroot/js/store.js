function byId(id) {
    return document.getElementById(id);
}

const SW_STEPS = [
    { label: 'Account Owner', sub: 'Step 1', ico: '👤' },
    { label: 'Store Details & Location', sub: 'Step 2', ico: '🏪' },
    { label: 'Contact & Payments', sub: 'Step 3', ico: '📞' },
    { label: 'Business & Docs', sub: 'Step 4', ico: '📄' }
];

let swCur = 1;
let swDelChoice = null;
let swBizChoice = null;
let swStoreMap = null;
let swStoreMarker = null;

function swRenderSidebar() {
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
      </div>
    `;
    });

    const panel = byId('swSbSteps');
    if (panel) panel.innerHTML = h;
}

function swRenderDots() {
    let h = '';

    for (let i = 1; i <= 4; i++) {
        const cls = i < swCur ? 'done' : i === swCur ? 'active' : '';
        h += `<div class="sw-dot ${cls}"></div>`;
    }

    const d = byId('swDots');
    if (d) d.innerHTML = h;
}

function swUpdateProgress() {
    const steps = 4;
    const pct = swCur > steps ? 100 : Math.round((swCur / steps) * 100);

    const fill = document.querySelector('.prog-fill');
    if (fill) fill.style.width = pct + '%';
}

function swGo(step) {
    document.querySelectorAll('#storeWizardContainer .sw-screen').forEach(s => s.classList.remove('on'));

    if (step > 4) {
        const s = byId('sw-ssuccess');
        if (s) s.classList.add('on');
        swCur = 5;
    } else {
        const s = byId('sw-s' + step);
        if (s) s.classList.add('on');
        swCur = step;
    }

    swRenderSidebar();
    swRenderDots();
    swUpdateProgress();

    window.scrollTo({ top: 0, behavior: 'smooth' });
}

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

    const bs = document.querySelectorAll('#sw-ps .sw-pb');
    const cl = ['w', 'm', 's'];

    bs.forEach((b, i) => {
        b.className = 'sw-pb' + (i < s ? ' ' + cl[s - 1] : '');
    });

    const pl = byId('sw-pl');
    if (pl) pl.textContent = s > 0 ? ['Weak', 'Medium', 'Strong'][s - 1] : 'Enter a password';
}

function swN1() {
    const ok =
        swReq('sw-fn') &
        swReq('sw-ln') &
        swVEmail('sw-oemail') &
        swReq('sw-ophone');

    const p1 = byId('sw-opwd') ? byId('sw-opwd').value : '';
    const p2 = byId('sw-opwd2') ? byId('sw-opwd2').value : '';

    if (!ok) return;

    if (p1.length < 8) {
        const p = byId('sw-opwd');
        if (p) p.classList.add('err');
        return;
    }

    if (p1 !== p2) {
        const p = byId('sw-opwd2');
        if (p) p.classList.add('err');
        const e = byId('sw-e-opwd2');
        if (e) e.classList.add('show');
        return;
    }

    window.location.href = '/Account/Stor_step2';
}

function swN2() {
    const ok = swReq('sw-sname') & swReq('sw-scity');

    const lat = byId('sw-lat') ? byId('sw-lat').value.trim() : '';
    const lng = byId('sw-lng') ? byId('sw-lng').value.trim() : '';

    if (!lat || !lng) {
        const e = byId('sw-e-map');
        if (e) e.classList.add('show');
    }

    if (!ok || !lat || !lng) return;

    window.location.href = '/Account/Stor_step3';
}

function swN3() {
    const ok = swVEmail('sw-semail') & swReq('sw-sphone');
    if (!ok) return;

    window.location.href = '/Account/Stor_step4';
}

function swFinish() {
    const t1 = byId('sw-t1') ? byId('sw-t1').checked : false;
    const t2 = byId('sw-t2') ? byId('sw-t2').checked : false;
    const t3 = byId('sw-t3') ? byId('sw-t3').checked : false;

    ['sw-t1', 'sw-t2', 'sw-t3'].forEach((id, i) => {
        const checks = [t1, t2, t3];
        if (!checks[i]) {
            const el = byId(id);
            if (el) {
                el.style.outline = '2px solid var(--red)';
                setTimeout(() => {
                    el.style.outline = '';
                }, 1200);
            }
        }
    });

    if (!t1 || !t2 || !t3) return;

    window.location.href = '/Account/Stor_success';
}

function swSetDel(val) {
    swDelChoice = val;

    const yes = byId('swDelYes');
    const no = byId('swDelNo');
    const extra = byId('swDelExtra');

    if (yes) yes.classList.remove('on');
    if (no) no.classList.remove('on');

    if (val) {
        if (yes) yes.classList.add('on');
        if (extra) extra.style.display = 'block';
    } else {
        if (no) no.classList.add('on');
        if (extra) extra.style.display = 'none';
    }
}

function swSelectBiz(type) {
    swBizChoice = type;

    const comm = byId('swBizComm');
    const sub = byId('swBizSub');
    const commExtra = byId('swCommExtra');
    const subExtra = byId('swSubExtra');

    if (comm) comm.classList.remove('on');
    if (sub) sub.classList.remove('on');

    if (type === 'commission') {
        if (comm) comm.classList.add('on');
        if (commExtra) commExtra.style.display = 'grid';
        if (subExtra) subExtra.style.display = 'none';
    } else {
        if (sub) sub.classList.add('on');
        if (subExtra) subExtra.style.display = 'block';
        if (commExtra) commExtra.style.display = 'none';
    }
}

function swSelPlan(card) {
    document.querySelectorAll('#storeWizardContainer .sw-plan-card').forEach(c => c.classList.remove('on'));
    if (card) card.classList.add('on');
}

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

function swAddImg(thumb) {
    const inp = document.createElement('input');
    inp.type = 'file';
    inp.accept = 'image/*';
    inp.style.display = 'none';

    document.body.appendChild(inp);
    inp.click();

    inp.onchange = () => {
        if (!inp.files || !inp.files[0]) {
            document.body.removeChild(inp);
            return;
        }

        const reader = new FileReader();
        reader.onload = e => {
            const img = document.createElement('img');
            img.src = e.target.result;

            thumb.innerHTML = '';
            thumb.appendChild(img);

            const rm = document.createElement('button');
            rm.className = 'sw-thumb-rm';
            rm.textContent = '✕';
            rm.type = 'button';

            rm.onclick = (ev) => {
                ev.stopPropagation();
                thumb.innerHTML = '➕';
                thumb.classList.remove('filled');
                thumb.onclick = () => swAddImg(thumb);
            };

            thumb.appendChild(rm);
            thumb.classList.add('filled');
            thumb.onclick = null;
        };

        reader.readAsDataURL(inp.files[0]);
        document.body.removeChild(inp);
    };
}

function swInitStoreMap() {
    const mapEl = byId('swStoreMap');
    const mapNote = byId('swMapNote');

    if (!mapEl) {
        console.error('swStoreMap element not found');
        return;
    }

    if (typeof L === 'undefined') {
        console.error('Leaflet library did not load');
        if (mapNote) {
            mapNote.textContent = '⚠ Map library failed to load. Check Leaflet script/link.';
        }
        return;
    }

    if (swStoreMap) {
        swStoreMap.remove();
        swStoreMap = null;
    }

    swStoreMap = L.map('swStoreMap').setView([31.9539, 35.9106], 8);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(swStoreMap);

    swStoreMap.on('click', function (e) {
        const lat = e.latlng.lat;
        const lng = e.latlng.lng;

        if (swStoreMarker) {
            swStoreMap.removeLayer(swStoreMarker);
        }

        swStoreMarker = L.marker([lat, lng]).addTo(swStoreMap);
        swStoreMarker.bindPopup('Selected store location').openPopup();

        const latInput = byId('sw-lat');
        const lngInput = byId('sw-lng');
        const mapErr = byId('sw-e-map');

        if (latInput) latInput.value = lat.toFixed(6);
        if (lngInput) lngInput.value = lng.toFixed(6);

        if (mapNote) {
            mapNote.textContent = `📍 Selected location: ${lat.toFixed(6)}, ${lng.toFixed(6)}`;
        }

        if (mapErr) mapErr.classList.remove('show');
    });

    setTimeout(() => {
        swStoreMap.invalidateSize();
    }, 500);
}

document.addEventListener('DOMContentLoaded', () => {
    const body = document.body;

    if (body.classList.contains('store-step-1')) swCur = 1;
    else if (body.classList.contains('store-step-2')) swCur = 2;
    else if (body.classList.contains('store-step-3')) swCur = 3;
    else if (body.classList.contains('store-step-4')) swCur = 4;
    else if (body.classList.contains('store-success')) swCur = 5;

    swRenderSidebar();
    swRenderDots();
    swUpdateProgress();

    if (body.classList.contains('store-step-2')) {
        swInitStoreMap();
    }
});