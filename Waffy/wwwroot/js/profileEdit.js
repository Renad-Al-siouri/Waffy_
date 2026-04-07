const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

function initNavbar() {
    if (servicesToggle && servicesDropdown) {
        servicesToggle.addEventListener("click", function (e) {
            e.stopPropagation();
            servicesDropdown.classList.toggle("open");
        });

        document.addEventListener("click", function (e) {
            if (!servicesDropdown.contains(e.target)) {
                servicesDropdown.classList.remove("open");
            }
        });
    }

    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            navLinks.classList.toggle("open");
        });

        document.addEventListener("click", function (e) {
            if (!navLinks.contains(e.target) && !menuBtn.contains(e.target)) {
                navLinks.classList.remove("open");
            }
        });
    }

    if (mobileServicesToggle && mobileServicesBlock) {
        mobileServicesToggle.addEventListener("click", function (e) {
            e.stopPropagation();
            mobileServicesBlock.classList.toggle("open");
        });
    }
}

function switchTab(id) {
    document.querySelectorAll(".tab-pane").forEach((pane) => pane.classList.remove("active"));
    document.querySelectorAll(".sb-item").forEach((item) => item.classList.remove("active"));

    document.getElementById("tab-" + id)?.classList.add("active");
    document.querySelector(`.sb-item[onclick*="${id}"]`)?.classList.add("active");

    if (id === "wallet") renderTx();
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function changeAvatar(inp) {
    if (!inp.files || !inp.files[0]) return;

    const reader = new FileReader();
    reader.onload = (e) => {
        const src = e.target.result;
        const sidebarAvatar = document.getElementById("sidebarAvatar");
        const bigAvatar = document.getElementById("bigAvatar");

        if (sidebarAvatar) sidebarAvatar.innerHTML = `<img src="${src}" alt="Avatar">`;
        if (bigAvatar) bigAvatar.innerHTML = `<img src="${src}" alt="Avatar">`;
    };

    reader.readAsDataURL(inp.files[0]);
}

function resetAvatar() {
    ["sidebarAvatar", "bigAvatar"].forEach((id) => {
        const el = document.getElementById(id);
        if (el) el.innerHTML = "<span>👤</span>";
    });

    const avatarFile = document.getElementById("avatarFile");
    if (avatarFile) avatarFile.value = "";

    showToast("Profile photo removed");
}

function syncName() {
    const nameInput = document.getElementById("f-name");
    const sidebarName = document.getElementById("sidebarName");
    if (nameInput && sidebarName) {
        sidebarName.textContent = nameInput.value || "Your Name";
    }
}

function syncEmail() {
    const emailInput = document.getElementById("f-email");
    const sidebarEmail = document.getElementById("sidebarEmail");
    if (emailInput && sidebarEmail) {
        sidebarEmail.textContent = emailInput.value || "your@email.com";
    }
}

function saveProfile() {
    syncName();
    syncEmail();
    showToast("✅ Profile updated successfully!");
}

function selAmt(btn, val) {
    document.querySelectorAll("#amtGrid .amt-btn").forEach((b) => b.classList.remove("active"));
    btn.classList.add("active");

    const customAmtWrap = document.getElementById("customAmtWrap");
    if (customAmtWrap) {
        customAmtWrap.style.display = val === "custom" ? "block" : "none";
    }
}

function selTopup(btn, method) {
    document.querySelectorAll("#topupGrid .topup-btn").forEach((b) => b.classList.remove("active"));
    btn.classList.add("active");

    ["card", "cliq", "cash"].forEach((m) => {
        const el = document.getElementById("topup-" + m);
        if (el) el.style.display = m === method ? "block" : "none";
    });
}

function showToast(msg) {
    const t = document.getElementById("toast");
    if (!t) return;
    t.textContent = msg;
    t.classList.add("show");
    clearTimeout(t._toastTimer);
    t._toastTimer = setTimeout(() => t.classList.remove("show"), 3000);
}

function renderTx() {
    const txList = document.getElementById("txList");
    if (!txList) return;

    const data = [
        { ico: "💳", label: "card", name: "Wallet Top Up", sub: "Today · Visa ending 4242", amt: "+10.00", cls: "plus" },
        { ico: "🛍️", label: "shop", name: "Store Order", sub: "Pet Friends Store · Mar 8", amt: "-18.50", cls: "minus" },
        { ico: "🎁", label: "reward", name: "Finder Reward", sub: "Lost pet reward · Feb 28", amt: "+10.00", cls: "plus" },
        { ico: "💉", label: "clinic", name: "Clinic Payment", sub: "Al Amanah Clinic · Feb 20", amt: "-12.00", cls: "minus" }
    ];

    txList.innerHTML = data.map(t => `
    <div class="tx-row" style="display:flex;align-items:center;justify-content:space-between;gap:12px;padding:12px 0;border-bottom:1px solid var(--line);">
      <div style="display:flex;align-items:center;gap:12px;min-width:0;">
        <div style="width:42px;height:42px;border-radius:12px;background:#f6f0e6;display:flex;align-items:center;justify-content:center;font-size:1.1rem;flex-shrink:0;">${t.ico}</div>
        <div class="tx-info">
          <h4 style="font-size:.88rem;font-weight:800;color:var(--text);margin-bottom:3px;">${t.name}</h4>
          <p style="font-size:.78rem;color:var(--muted);font-weight:700;">${t.sub}</p>
        </div>
      </div>
      <div class="tx-amt ${t.cls}" style="font-family:'Fredoka',sans-serif;font-weight:600;color:${t.cls === "plus" ? "#4e7b2f" : "#b45a4c"};">
        ${t.amt} <span style="font-size:.72rem;font-weight:700;color:var(--muted)">JOD</span>
      </div>
    </div>
  `).join("");
}

function openAdoptionEditModal() {
    const modal = document.getElementById("adoptionEditModal");
    if (modal) modal.classList.add("show");
    else showToast("Adoption edit modal is not connected yet.");
}

function closeAdoptionEditModal() {
    const modal = document.getElementById("adoptionEditModal");
    if (modal) modal.classList.remove("show");
}

function saveAdoptionEdit() {
    const name = document.getElementById("editAdoptionName");
    const age = document.getElementById("editAdoptionAge");
    const city = document.getElementById("editAdoptionCity");
    const gender = document.getElementById("editAdoptionGender");
    const vaccinated = document.getElementById("editAdoptionVaccinated");
    const neutered = document.getElementById("editAdoptionNeutered");
    const status = document.getElementById("editAdoptionStatus");

    const titleEl = document.getElementById("adoptionPetName");
    const infoEl = document.getElementById("adoptionPetInfo");
    const genderChip = document.getElementById("adoptionGenderChip");
    const vaccChip = document.getElementById("adoptionVaccChip");
    const neutChip = document.getElementById("adoptionNeuteredChip");
    const statusChip = document.getElementById("adoptionStatusChip");
    const badge = document.getElementById("adoptionBadge");

    if (titleEl && name && age) titleEl.textContent = `${name.value} · ${age.value}`;
    if (infoEl && city) infoEl.textContent = `Updated listing · ${city.value}`;
    if (genderChip && gender) genderChip.textContent = gender.value;
    if (vaccChip && vaccinated) vaccChip.textContent = vaccinated.value;
    if (neutChip && neutered) neutChip.textContent = neutered.value;
    if (statusChip && status) statusChip.textContent = status.value;
    if (badge && status) badge.textContent = status.value;

    closeAdoptionEditModal();
    showToast("✅ Adoption listing updated!");
}

function closeAdoptionAd() {
    const badge = document.getElementById("adoptionBadge");
    const chip = document.getElementById("adoptionStatusChip");
    if (badge) {
        badge.textContent = "Closed";
        badge.className = "badge badge-grey";
    }
    if (chip) chip.textContent = "Closed";
    showToast("Adoption ad closed");
}

function openComplaintModal() {
    const modal = document.getElementById("complaintModal");
    if (modal) modal.classList.add("show");
}

function closeComplaintModal() {
    const modal = document.getElementById("complaintModal");
    if (modal) modal.classList.remove("show");
}

function setComplaintType(type, btn) {
    const typeInput = document.getElementById("complaintType");
    const clinicWrap = document.getElementById("clinicTargetWrap");
    const storeWrap = document.getElementById("storeTargetWrap");
    const routeNote = document.getElementById("complaintRouteNote");

    document.querySelectorAll(".complaint-type-btn").forEach((b) => {
        b.classList.remove("active");
    });

    if (btn) btn.classList.add("active");
    if (typeInput) typeInput.value = type;

    if (clinicWrap) clinicWrap.classList.remove("show");
    if (storeWrap) storeWrap.classList.remove("show");

    if (type === "clinic") {
        if (clinicWrap) clinicWrap.classList.add("show");
        if (routeNote) routeNote.textContent = "Routing: Clinic Admin + Super Admin";
    } else if (type === "store") {
        if (storeWrap) storeWrap.classList.add("show");
        if (routeNote) routeNote.textContent = "Routing: Store Admin + Super Admin";
    } else {
        if (routeNote) routeNote.textContent = "Routing: Super Admin Only";
    }
}

function submitComplaint() {
    const title = document.getElementById("complaintTitle")?.value.trim();
    const details = document.getElementById("complaintDetails")?.value.trim();

    if (!title || !details) {
        showToast("Please fill in the complaint title and details.");
        return;
    }

    closeComplaintModal();
    showToast("✅ Complaint submitted successfully!");
}

window.addEventListener("click", (e) => {
    const complaintModal = document.getElementById("complaintModal");
    const adoptionEditModal = document.getElementById("adoptionEditModal");

    if (complaintModal && e.target === complaintModal) {
        complaintModal.classList.remove("show");
    }

    if (adoptionEditModal && e.target === adoptionEditModal) {
        adoptionEditModal.classList.remove("show");
    }
});

document.addEventListener("DOMContentLoaded", () => {
    initNavbar();
    renderTx();
    syncName();
    syncEmail();

    document.querySelectorAll(".modal-overlay").forEach((modal) => {
        modal.classList.remove("show");
    });

    const clinicWrap = document.getElementById("clinicTargetWrap");
    const storeWrap = document.getElementById("storeTargetWrap");

    if (clinicWrap) clinicWrap.classList.add("show");
    if (storeWrap) storeWrap.classList.remove("show");
});