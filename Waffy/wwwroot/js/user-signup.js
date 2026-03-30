function byId(id) { return document.getElementById(id); }
function qsa(selector) { return document.querySelectorAll(selector); }

const STEPS = [
  { label: 'Account Information', sub: 'Step 1', ico: '👤' },
  { label: 'Contact & Location', sub: 'Step 2', ico: '📍' },
  { label: 'Finish', sub: 'Step 3', ico: '🎉' }
];

let cur = 1;
let userPets = [];

function renderSB() {
  const panel = byId('stepsPanel');
  const dotsEl = byId('dots');
  const pf = byId('pf');

  let html = `
    <div style="font-size:.68rem;font-weight:800;color:var(--muted);letter-spacing:1px;text-transform:uppercase;margin-bottom:.8rem">
      Registration Steps
    </div>
  `;

  STEPS.forEach((s, i) => {
    const n = i + 1;
    const cls = n < cur ? 'done' : n === cur ? 'active' : '';
    html += `
      <div class="si ${cls}">
        <div class="si-num">${n < cur ? '✓' : n}</div>
        <div class="si-txt">
          <div class="si-lbl">${s.ico} ${s.label}</div>
          <div class="si-en">${s.sub}</div>
        </div>
      </div>
    `;
  });

  if (panel) panel.innerHTML = html;

  if (dotsEl) {
    let dots = '';
    STEPS.forEach((_, i) => {
      const n = i + 1;
      dots += `<div class="dot ${n < cur ? 'done' : n === cur ? 'active' : ''}"></div>`;
    });
    dotsEl.innerHTML = dots;
  }

  if (pf) {
    pf.style.width = (cur > 3 ? 100 : Math.round(((cur - 1) / 3) * 92 + 8)) + '%';
  }
}

function ce(el) {
  if (!el) return;
  el.classList.remove('err');
  const err = byId('e-' + el.id);
  if (err) err.classList.remove('show');
}

function req(id) {
  const el = byId(id);
  if (!el || !el.value.trim()) {
    if (el) el.classList.add('err');
    const err = byId('e-' + id);
    if (err) err.classList.add('show');
    return false;
  }
  return true;
}

function vEmail(id) {
  const el = byId(id);
  if (!el) return false;

  const ok = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(el.value || '');
  if (!ok) {
    el.classList.add('err');
    const err = byId('e-' + id);
    if (err) err.classList.add('show');
  }
  return ok;
}

function pwdStr(inp) {
  if (!inp) return;

  const v = inp.value;
  let s = 0;

  if (v.length >= 8) s++;
  if (/[0-9]/.test(v)) s++;
  if (/[A-Z]/.test(v) || /[!@#$%^&*]/.test(v)) s++;

  const cls = ['w', 'm', 's'];
  qsa('#pwdStr .pb').forEach((b, i) => {
    b.className = 'pb' + (i < s ? ' ' + cls[s - 1] : '');
  });

  const pl = byId('pwdLbl');
  if (pl) {
    pl.textContent = s > 0 ? ['Weak', 'Medium', 'Strong'][s - 1] : 'Enter a password';
  }
}

function n1() {
  const ok = req('fullName') & vEmail('email');
  const pwd = byId('pwd');
  const pwd2 = byId('pwd2');

  if (!pwd || !pwd2 || !ok) return;

  if (pwd.value.length < 8) {
    pwd.classList.add('err');
    return;
  }

  if (pwd.value !== pwd2.value) {
    pwd2.classList.add('err');
    const e = byId('e-pwd2');
    if (e) e.classList.add('show');
    return;
  }

  window.location.href = 'user-step2.html';
}

function n2() {
  const phone = byId('phone');
  const city = byId('city');
  let ok = true;

  if (!phone || !phone.value.trim()) {
    if (phone) phone.classList.add('err');
    const e = byId('e-phone');
    if (e) e.classList.add('show');
    ok = false;
  }

  if (!city || !city.value.trim()) {
    if (city) city.classList.add('err');
    const e = byId('e-city');
    if (e) e.classList.add('show');
    ok = false;
  }

  if (!ok) return;
  window.location.href = 'user-step3.html';
}

function finishAndGo() {
  const t = byId('terms1');
  if (!t) return;

  if (!t.checked) {
    t.style.outline = '2px solid red';
    setTimeout(() => {
      t.style.outline = '';
    }, 1200);
    return;
  }

  window.location.href = 'user-success.html';
}

function previewAvatar(inp) {
  if (!inp || !inp.files || !inp.files[0]) return;

  const img = byId('avatarImg');
  const em = byId('avatarEmoji');
  if (!img || !em) return;

  const r = new FileReader();
  r.onload = e => {
    img.src = e.target.result;
    img.style.display = 'block';
    em.style.display = 'none';
  };
  r.readAsDataURL(inp.files[0]);
}

function showPetForm() {
  const form = byId('petInlineForm');
  const noPet = byId('noPetState');
  if (form) form.classList.remove('hidden');
  if (noPet) noPet.style.display = 'none';
}

function cancelPetForm() {
  clearPetForm();
  const form = byId('petInlineForm');
  const noPet = byId('noPetState');

  if (form) form.classList.add('hidden');
  if (userPets.length === 0 && noPet) {
    noPet.style.display = 'block';
  }
}

function savePet() {
  const name = byId('petName1')?.value.trim() || '';
  const type = byId('petType1')?.value || '';
  const breed = byId('petBreed1')?.value.trim() || '';
  const age = byId('petAge1')?.value.trim() || '';
  const gender = byId('petGender1')?.value || '';
  const color = byId('petColor1')?.value.trim() || '';
  const health = byId('petHealth1')?.value || '';
  const neuter = byId('petNeuter1')?.value || '';
  const vaccinated = byId('petVaccinated1')?.value || '';
  const vaccDate = byId('petVaccDate1')?.value || '';
  const problems = byId('petProblems1')?.value.trim() || '';
  const traits = byId('petTraits1')?.value.trim() || '';
  const energy = byId('petEnergy1')?.value || '';
  const children = byId('petChildren1')?.value || '';
  const otherPets = byId('petOtherPets1')?.value || '';
  const desc = byId('petDesc1')?.value.trim() || '';

  if (!name || !type || !breed || !age || !gender || !health || !neuter || !vaccinated) {
    alert('Please fill in all required pet fields.');
    return;
  }

  const typeMap = {
    dog: '🐕 Dog',
    cat: '🐈 Cat',
    bird: '🐦 Bird',
    rabbit: '🐇 Rabbit',
    other: '🐾 Other'
  };

  userPets.push({
    id: Date.now(),
    name,
    typeLabel: typeMap[type] || '🐾 Other',
    breed,
    age,
    gender,
    color,
    health,
    neuter,
    vaccinated,
    vaccDate,
    problems,
    traits,
    energy,
    children,
    otherPets,
    desc
  });

  renderPetsList();
  clearPetForm();

  const petForm = byId('petInlineForm');
  const noPet = byId('noPetState');
  const petsAdded = byId('petsAdded');

  if (petForm) petForm.classList.add('hidden');
  if (noPet) noPet.style.display = 'none';
  if (petsAdded) petsAdded.style.display = 'block';
}

function clearPetForm() {
  [
    'petName1','petType1','petBreed1','petAge1','petGender1','petColor1',
    'petHealth1','petNeuter1','petVaccinated1','petVaccDate1',
    'petProblems1','petTraits1','petDesc1'
  ].forEach(id => {
    if (byId(id)) byId(id).value = '';
  });

  if (byId('petEnergy1')) byId('petEnergy1').value = 'Medium';
  if (byId('petChildren1')) byId('petChildren1').value = 'Yes';
  if (byId('petOtherPets1')) byId('petOtherPets1').value = 'Yes';
}

function renderPetsList() {
  const petsList = byId('petsList');
  const petsAdded = byId('petsAdded');
  const noPetState = byId('noPetState');

  if (!petsList || !petsAdded || !noPetState) return;

  if (!userPets.length) {
    petsList.innerHTML = '';
    petsAdded.style.display = 'none';
    noPetState.style.display = 'block';
    return;
  }

  petsAdded.style.display = 'block';
  noPetState.style.display = 'none';

  petsList.innerHTML = userPets.map(pet => `
    <div class="pet-item">
      <div class="pet-item-top">
        <div>
          <div class="pet-item-name">${pet.typeLabel} ${pet.name}</div>
          <div class="pet-item-sub">${pet.breed} • ${pet.age} • ${pet.gender}</div>
        </div>
        <button type="button" class="pet-remove-btn" onclick="removePet(${pet.id})">Remove</button>
      </div>

      <div class="pet-item-grid">
        <div><strong>Color:</strong> ${pet.color || '-'}</div>
        <div><strong>Health:</strong> ${pet.health || '-'}</div>
        <div><strong>Vaccinated:</strong> ${pet.vaccinated || '-'}</div>
        <div><strong>Neutered:</strong> ${pet.neuter || '-'}</div>
        <div><strong>Vaccination Date:</strong> ${pet.vaccDate || '-'}</div>
        <div><strong>Problems:</strong> ${pet.problems || '-'}</div>
        <div><strong>Traits:</strong> ${pet.traits || '-'}</div>
        <div><strong>Energy:</strong> ${pet.energy || '-'}</div>
        <div><strong>Good with Children:</strong> ${pet.children || '-'}</div>
        <div><strong>Good with Pets:</strong> ${pet.otherPets || '-'}</div>
      </div>

      ${pet.desc ? `<div class="pet-item-desc"><strong>Description:</strong> ${pet.desc}</div>` : ''}
    </div>
  `).join('');
}

function removePet(id) {
  userPets = userPets.filter(pet => pet.id !== id);
  renderPetsList();
}

document.addEventListener('DOMContentLoaded', () => {
  const body = document.body;

  if (body.classList.contains('user-step-1')) cur = 1;
  else if (body.classList.contains('user-step-2')) cur = 2;
  else if (body.classList.contains('user-step-3')) cur = 3;
  else if (body.classList.contains('user-success')) cur = 4;

  renderSB();
});