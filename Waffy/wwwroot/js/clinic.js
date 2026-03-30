function byId(id) {
  return document.getElementById(id);
}

let cwClinicStep = 1;
let cwDoctorStep = 1;
let cwSelectedPlan = 'basic';

const CW_CLINIC_TOTAL = 5;
const CW_DOCTOR_TOTAL = 4;

function cwUpdateSidePanel(type) {
  const list = byId('cwStepsList');
  if (!list) return;

  if (type === 'initial') {
    list.innerHTML = `
      <div class="cw-step-item">
        <div class="cw-step-num active">1</div>
        <div class="cw-step-text">Choose Account Type</div>
      </div>
    `;
  } else if (type === 'clinic') {
    const steps = [
      'Clinic Information',
      'Schedule & Services',
      'Subscription Plan',
      'Official Documents',
      'Manager Info'
    ];

    list.innerHTML = steps.map((s, i) => `
      <div class="cw-step-item">
        <div class="cw-step-num ${i === 0 ? 'active' : ''}" id="cwcs${i + 1}">${i + 1}</div>
        <div class="cw-step-text">${s}</div>
      </div>
    `).join('');
  } else if (type === 'doctor') {
    const steps = [
      'Invite Code',
      'Personal Info',
      'Specialty & Schedule',
      'Official Documents'
    ];

    list.innerHTML = steps.map((s, i) => `
      <div class="cw-step-item">
        <div class="cw-step-num ${i === 0 ? 'active' : ''}" id="cwds${i + 1}">${i + 1}</div>
        <div class="cw-step-text">${s}</div>
      </div>
    `).join('');
  }
}

function cwUpdateSidePanelStep(prefix, current, total) {
  for (let i = 1; i <= total; i++) {
    const el = byId(`${prefix}${i}`);
    if (!el) continue;

    el.classList.remove('active', 'done');

    if (i < current) {
      el.classList.add('done');
    } else if (i === current) {
      el.classList.add('active');
    }
  }
}

function cwUpdateIndicators(containerId, current, total) {
  const inds = document.querySelectorAll(`#${containerId} .cw-ind`);

  inds.forEach((ind, i) => {
    const n = i + 1;
    ind.classList.remove('active', 'done');

    if (n < current) {
      ind.classList.add('done');
      ind.querySelector('.cw-dot').textContent = '✓';
    } else if (n === current) {
      ind.classList.add('active');
      ind.querySelector('.cw-dot').textContent = n;
    } else {
      ind.querySelector('.cw-dot').textContent = n;
    }
  });
}

function cwSelectSubRole(type, el) {
  document.querySelectorAll('#clinicWizardContainer .cw-sub-role-card').forEach(c => {
    c.classList.remove('selected');
  });

  if (el) el.classList.add('selected');

  setTimeout(() => {
    byId('cwRoleSelector').style.display = 'none';

    if (type === 'clinic-owner') {
      byId('cwClinicForm').classList.add('show');
      byId('cwDoctorForm').classList.remove('show');

      cwClinicStep = 1;
      cwUpdateIndicators('cwClinicIndicators', 1, CW_CLINIC_TOTAL);
      byId('cwClinicCounter').textContent = 'Step 1 of 5';

      document.querySelectorAll('[id^="cc-"]').forEach(s => {
        if (s.id === 'cc-1') s.classList.remove('hidden');
        else s.classList.add('hidden');
      });

      cwUpdateSidePanel('clinic');
    } else {
      byId('cwDoctorForm').classList.add('show');
      byId('cwClinicForm').classList.remove('show');

      cwDoctorStep = 1;
      cwUpdateIndicators('cwDoctorIndicators', 1, CW_DOCTOR_TOTAL);
      byId('cwDoctorCounter').textContent = 'Step 1 of 4';

      document.querySelectorAll('[id^="cd-"]').forEach(s => {
        if (s.id === 'cd-1') s.classList.remove('hidden');
        else s.classList.add('hidden');
      });

      cwUpdateSidePanel('doctor');
    }

    window.scrollTo({ top: 0, behavior: 'smooth' });
  }, 300);
}

function cwGoRoleSelector() {
  byId('cwRoleSelector').style.display = 'block';
  byId('cwClinicForm').classList.remove('show');
  byId('cwDoctorForm').classList.remove('show');
  byId('cwSuccess').classList.remove('show');

  document.querySelectorAll('#clinicWizardContainer .cw-sub-role-card').forEach(c => {
    c.classList.remove('selected');
  });

  cwUpdateSidePanel('initial');
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function cwClinicNext(from) {
  if (from === 4) {
    const n = byId('cc-clinicName');
    const c = byId('cc-city');
    const p = byId('cc-phone');

    if (byId('cwReviewName')) byId('cwReviewName').textContent = n && n.value ? n.value : '—';
    if (byId('cwReviewCity')) byId('cwReviewCity').textContent = c && c.value ? c.value : '—';
    if (byId('cwReviewPhone')) byId('cwReviewPhone').textContent = p && p.value ? p.value : '—';
  }

  cwClinicGo(from, from + 1);
}

function cwClinicGo(from, to) {
  byId(`cc-${from}`).classList.add('hidden');
  byId(`cc-${to}`).classList.remove('hidden');

  cwClinicStep = to;
  cwUpdateIndicators('cwClinicIndicators', to, CW_CLINIC_TOTAL);
  byId('cwClinicCounter').textContent = `Step ${to} of ${CW_CLINIC_TOTAL}`;
  cwUpdateSidePanelStep('cwcs', to, CW_CLINIC_TOTAL);

  window.scrollTo({ top: 60, behavior: 'smooth' });
}

function cwSubmitClinic() {
  const consent = byId('cwClinicConsent');
  if (!consent || !consent.checked) {
    consent.style.outline = '2px solid red';
    setTimeout(() => {
      consent.style.outline = '';
    }, 1200);
    return;
  }

  byId('cwClinicForm').classList.remove('show');
  byId('cwSuccess').classList.add('show');
  byId('cwSuccessIcon').textContent = '🏥';
  byId('cwSuccessTitle').textContent = 'Clinic Application Submitted!';
  byId('cwSuccessSub').textContent = 'The Waffy team will review your application and approve it within 24 hours.';

  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function cwDoctorNext(from) {
  if (from === 1) {
    const inv = byId('cwInviteInput');
    const suc = byId('cwInviteSuccess');

    if (!suc.classList.contains('show') && (!inv || inv.value.trim().length < 6)) {
      if (inv) inv.classList.add('error');
      return;
    }

    const code = byId('cwInviteInput');
    if (byId('cwReviewCode') && code) {
      byId('cwReviewCode').textContent = code.value.toUpperCase();
    }
  }

  cwDoctorGo(from, from + 1);
}

function cwDoctorGo(from, to) {
  byId(`cd-${from}`).classList.add('hidden');
  byId(`cd-${to}`).classList.remove('hidden');

  cwDoctorStep = to;
  cwUpdateIndicators('cwDoctorIndicators', to, CW_DOCTOR_TOTAL);
  byId('cwDoctorCounter').textContent = `Step ${to} of ${CW_DOCTOR_TOTAL}`;
  cwUpdateSidePanelStep('cwds', to, CW_DOCTOR_TOTAL);

  window.scrollTo({ top: 60, behavior: 'smooth' });
}

function cwSubmitDoctor() {
  const consent = byId('cwDoctorConsent');
  if (!consent || !consent.checked) {
    consent.style.outline = '2px solid red';
    setTimeout(() => {
      consent.style.outline = '';
    }, 1200);
    return;
  }

  byId('cwDoctorForm').classList.remove('show');
  byId('cwSuccess').classList.add('show');
  byId('cwSuccessIcon').textContent = '👨‍⚕️';
  byId('cwSuccessTitle').textContent = 'Doctor Application Submitted!';
  byId('cwSuccessSub').textContent = 'The Waffy team will review your application and you will receive approval within 24 hours via email.';

  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function cwVerifyCode() {
  const inp = byId('cwInviteInput');
  const suc = byId('cwInviteSuccess');
  const code = inp ? inp.value.toUpperCase().trim() : '';

  if (inp) inp.classList.remove('error');

  if (code === 'WAFFY-DR-4X9K' || code.length >= 8) {
    suc.classList.add('show');
    if (inp) {
      inp.style.borderColor = '#5a8f38';
      inp.style.background = '#edf7e5';
    }
  } else {
    if (inp) inp.classList.add('error');
    suc.classList.remove('show');
  }
}

function cwSelectPlan(el, plan) {
  document.querySelectorAll('#clinicWizardContainer .cw-plan').forEach(c => {
    c.classList.remove('active');
  });

  el.classList.add('active');
  cwSelectedPlan = plan;

  byId('cwPaymentDetails').style.display = plan === 'free' ? 'none' : 'block';

  if (byId('cwReviewPlan')) {
    const labels = {
      free: 'Free — 0 JOD',
      basic: 'Basic — 25 JOD/month',
      pro: 'Pro — 60 JOD/month'
    };
    byId('cwReviewPlan').textContent = labels[plan] || '—';
  }
}

/* Upload helpers */
function cwTrigUpload(id) {
  const el = byId(id);
  if (el) el.click();
}

function cwPreviewFile(inp, prevId, nameId) {
  if (!inp.files || !inp.files[0]) return;

  const n = byId(nameId);
  if (n) n.textContent = inp.files[0].name;

  const p = byId(prevId);
  if (p) p.classList.add('show');
}

function cwRemoveFile(fileId, prevId, nameId) {
  const f = byId(fileId);
  if (f) f.value = '';

  const n = byId(nameId);
  if (n) n.textContent = '';

  const p = byId(prevId);
  if (p) p.classList.remove('show');
}

function cwAddImg(thumb) {
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
      rm.className = 'cw-thumb-rm';
      rm.textContent = '✕';

      rm.onclick = ev => {
        ev.stopPropagation();
        thumb.innerHTML = '➕';
        thumb.classList.remove('filled');
        thumb.onclick = () => cwAddImg(thumb);
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
  cwUpdateSidePanel('initial');

  const defPlan = document.querySelector('#clinicWizardContainer .cw-plan.active');
  if (defPlan) cwSelectPlan(defPlan, 'basic');

  const inviteInput = byId('cwInviteInput');
  if (inviteInput) {
    inviteInput.addEventListener('keyup', e => {
      if (e.key === 'Enter') cwVerifyCode();
      e.target.value = e.target.value.toUpperCase().replace(/[^A-Z0-9\-]/g, '');
    });
  }
});