function byId(id) {
  return document.getElementById(id);
}

const SW_STEPS = [
  { label:'Account Owner', sub:'Step 1', ico:'👤' },
  { label:'Store Information', sub:'Step 2', ico:'🏪' },
  { label:'Contact & Location', sub:'Step 3', ico:'📍' },
  { label:'Media & Payments', sub:'Step 4', ico:'🖼' },
  { label:'Business & Docs', sub:'Step 5', ico:'📄' }
];

let swCur = 1;
let swDelChoice = null;
let swBizChoice = null;

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

  for (let i = 1; i <= 5; i++) {
    const cls = i < swCur ? 'done' : i === swCur ? 'active' : '';
    h += `<div class="sw-dot ${cls}"></div>`;
  }

  const d = byId('swDots');
  if (d) d.innerHTML = h;
}

function swUpdateProgress() {
  const pct = swCur > 5 ? 100 : Math.round((swCur - 1) / 5 * 95 + 5);

  const fill = byId('swProgFill');
  if (fill) fill.style.width = pct + '%';

  const badge = byId('swStepBadge');
  if (badge) badge.textContent = swCur <= 5 ? `Step ${swCur} of 5` : 'Complete ✓';
}

function swGo(step) {
  document.querySelectorAll('#storeWizardContainer .sw-screen').forEach(s => s.classList.remove('on'));

  if (step > 5) {
    const s = byId('sw-ssuccess');
    if (s) s.classList.add('on');
    swCur = 6;
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
  el.classList.remove('err');

  const e = byId('sw-e-' + el.id.replace('sw-', ''));
  if (e) e.classList.remove('show');
}

function swReq(id) {
  const el = byId(id);

  if (!el || !el.value.trim()) {
    if (el) el.classList.add('err');

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
  if (pl) pl.textContent = s > 0 ? ['Weak','Medium','Strong'][s - 1] : 'Enter a password';
}

function swN1() {
  const ok = swReq('sw-fn') & swReq('sw-ln') & swVEmail('sw-oemail') & swReq('sw-ophone');
  const p1 = byId('sw-opwd').value;
  const p2 = byId('sw-opwd2').value;

  if (!ok) return;

  if (p1.length < 8) {
    byId('sw-opwd').classList.add('err');
    return;
  }

  if (p1 !== p2) {
    byId('sw-opwd2').classList.add('err');
    const e = byId('sw-e-opwd2');
    if (e) e.classList.add('show');
    return;
  }

  swGo(2);
}

function swN2() {
  const ok = swReq('sw-sname');
  const hasType = !!document.querySelector('#storeWizardContainer .sw-type-btn.on');

  if (!hasType) {
    const e = byId('sw-e-type');
    if (e) e.classList.add('show');
  }

  if (!ok || !hasType) return;
  swGo(3);
}

function swN3() {
  const ok = swVEmail('sw-semail') & swReq('sw-sphone') & swReq('sw-country') & swReq('sw-scity') & swReq('sw-saddr');

  if (swDelChoice === null) {
    const y = byId('swDelYes');
    const n = byId('swDelNo');

    if (y) {
      y.style.borderColor = 'var(--sw-red)';
      setTimeout(() => { y.style.borderColor = ''; }, 1200);
    }

    if (n) {
      n.style.borderColor = 'var(--sw-red)';
      setTimeout(() => { n.style.borderColor = ''; }, 1200);
    }

    if (!ok) return;
    return;
  }

  if (!ok) return;
  swGo(4);
}

function swN4() {
  swGo(5);
}

function swFinish() {
  const t1 = byId('sw-t1').checked;
  const t2 = byId('sw-t2').checked;
  const t3 = byId('sw-t3').checked;

  ['sw-t1','sw-t2','sw-t3'].forEach((id, i) => {
    const checks = [t1, t2, t3];
    if (!checks[i]) {
      const el = byId(id);
      if (el) {
        el.style.outline = '2px solid var(--sw-red)';
        setTimeout(() => { el.style.outline = ''; }, 1200);
      }
    }
  });

  if (!t1 || !t2 || !t3) return;

  const ref = 'WAFFY-STR-' + Math.random().toString(36).slice(2,6).toUpperCase() + Math.floor(Math.random()*1000);
  const rn = byId('swReqNum');
  if (rn) rn.textContent = ref;

  swGo(6);
}

function swSetDel(yes) {
  swDelChoice = yes;

  const y = byId('swDelYes');
  const n = byId('swDelNo');
  const ex = byId('swDelExtra');

  if (y) y.classList.toggle('on', yes);
  if (n) n.classList.toggle('on', !yes);
  if (ex) ex.classList.toggle('show', yes);
}

function swSelectBiz(type) {
  swBizChoice = type;

  const comm = byId('swBizComm');
  const sub = byId('swBizSub');
  const commEx = byId('swCommExtra');
  const subEx = byId('swSubExtra');

  if (comm) comm.classList.toggle('on', type === 'commission');
  if (sub) sub.classList.toggle('on', type === 'subscription');
  if (commEx) commEx.classList.toggle('show', type === 'commission');
  if (subEx) subEx.classList.toggle('show', type === 'subscription');
}

function swSelPlan(card) {
  document.querySelectorAll('#storeWizardContainer .sw-plan-card').forEach(c => c.classList.remove('on'));
  card.classList.add('on');
}

/* Upload helpers */
function swTrigUpload(id) {
  const el = byId(id);
  if (el) el.click();
}

function swPreviewFile(inp, prevId, nameId) {
  if (!inp.files || !inp.files[0]) return;

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

document.addEventListener('DOMContentLoaded', () => {
  const body = document.body;

  if (body.classList.contains('store-step-1')) swCur = 1;
  else if (body.classList.contains('store-step-2')) swCur = 2;
  else if (body.classList.contains('store-step-3')) swCur = 3;
  else if (body.classList.contains('store-step-4')) swCur = 4;
  else if (body.classList.contains('store-step-5')) swCur = 5;
  else if (body.classList.contains('store-success')) swCur = 6;

  swRenderSidebar();
  swRenderDots();
  swUpdateProgress();
});