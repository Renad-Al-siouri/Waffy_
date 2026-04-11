// ════════════════════════════════════════════════════════
//  profileShared.js  —  shared across all profile pages
// ════════════════════════════════════════════════════════

// ── Toast notification ───────────────────────────────────
function showToast(msg, duration = 3000) {
    const t = document.getElementById('toast');
    if (!t) return;
    t.textContent = msg;
    t.classList.add('show');
    clearTimeout(t._timer);
    t._timer = setTimeout(() => t.classList.remove('show'), duration);
}

// ── Mobile nav toggle ────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {

    // Mobile hamburger
    const menuBtn = document.getElementById('menuBtn');
    const navLinks = document.getElementById('navLinks');
    if (menuBtn && navLinks) {
        menuBtn.addEventListener('click', e => {
            e.stopPropagation();
            navLinks.classList.toggle('open');
        });
        document.addEventListener('click', e => {
            if (!navLinks.contains(e.target) && e.target !== menuBtn)
                navLinks.classList.remove('open');
        });
    }

    // Desktop services dropdown
    const servicesDropdown = document.getElementById('servicesDropdown');
    const servicesToggle = document.getElementById('servicesToggle');
    if (servicesDropdown && servicesToggle) {
        servicesToggle.addEventListener('click', e => {
            e.stopPropagation();
            servicesDropdown.classList.toggle('open');
        });
        document.addEventListener('click', () => servicesDropdown.classList.remove('open'));
    }

    // Mobile services dropdown
    const mobileToggle = document.getElementById('mobileServicesToggle');
    const mobileBlock = document.getElementById('mobileServicesBlock');
    if (mobileToggle && mobileBlock) {
        mobileToggle.addEventListener('click', () => mobileBlock.classList.toggle('open'));
    }

    // Show TempData toast if present
    const successMsg = document.body.dataset.success;
    if (successMsg) showToast('✅ ' + successMsg);

    const errorMsg = document.body.dataset.error;
    if (errorMsg) showToast('❌ ' + errorMsg);
});

// ── Avatar change (sidebar) ──────────────────────────────
function changeAvatar(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = e => {
            const sbAv = document.getElementById('sidebarAvatar');
            if (sbAv) sbAv.innerHTML = `<img src="${e.target.result}" alt="Profile">`;
            const bigAv = document.getElementById('bigAvatar');
            if (bigAv) bigAv.innerHTML = `<img src="${e.target.result}" alt="Profile">`;
        };
        reader.readAsDataURL(input.files[0]);
    }
}
