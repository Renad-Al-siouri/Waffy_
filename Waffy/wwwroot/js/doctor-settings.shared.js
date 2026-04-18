/* =========================================================
   WAFFY — Doctor Settings Shared JS
   Used by: Personal-Information.html, Professional-Information.html,
            Work-Schedule.html, documents.html, Password.html, Notifications.html
========================================================= */

/* ── Global State ── */
let currentRole = localStorage.getItem("waffyClinicRole") || "doctor";

/* ── Helpers ── */
function byId(id) { return document.getElementById(id); }
function qs(selector, scope) { return (scope || document).querySelector(selector); }
function qsa(selector, scope) { return Array.from((scope || document).querySelectorAll(selector)); }

/* ── Toast ── */
function toast(msg, type) {
    let el = byId("toastMsg");
    if (!el) {
        el = document.createElement("div");
        el.id = "toastMsg";
        el.className = "toast-msg";
        document.body.appendChild(el);
    }
    el.textContent = msg;
    el.style.background = type === "error"
        ? "linear-gradient(180deg,#d98b83 0%,#c86a5c 100%)"
        : type === "warning"
            ? "linear-gradient(180deg,#d8a74e 0%,#bf7a23 100%)"
            : "linear-gradient(180deg,#9ab451 0%,#7d953d 100%)";
    el.classList.add("show");
    clearTimeout(el._t);
    el._t = setTimeout(() => el.classList.remove("show"), 2200);
}

/* ── Modal ── */
function openModal(id) {
    const el = byId(id);
    if (el) { el.classList.add("open"); el.classList.add("on"); }
}
function closeModal(id) {
    const el = byId(id);
    if (el) { el.classList.remove("open"); el.classList.remove("on"); }
}
function closeOnBg(e, id) {
    const modal = byId(id);
    if (modal && e.target === modal) closeModal(id);
}

/* ── Sidebar Toggle ── */
function toggleSidebar() {
    const sidebar = byId("sidebar") || qs(".wa-sb");
    const overlay = byId("sidebarOverlay");
    const shell = byId("waShell");
    if (window.innerWidth <= 992) {
        if (sidebar) sidebar.classList.toggle("mob-open");
        if (overlay) overlay.classList.toggle("show");
        document.body.classList.toggle("sidebar-open");
        return;
    }
    if (shell) shell.classList.toggle("mini");
}
function closeSidebar() {
    const sidebar = byId("sidebar") || qs(".wa-sb");
    const overlay = byId("sidebarOverlay");
    if (sidebar) sidebar.classList.remove("mob-open");
    if (overlay) overlay.classList.remove("show");
    document.body.classList.remove("sidebar-open");
}
function bindSidebarAutoClose() {
    qsa(".wa-sb .sb-row").forEach(item => {
        item.addEventListener("click", () => { if (window.innerWidth <= 992) closeSidebar(); });
    });
    window.addEventListener("resize", () => { if (window.innerWidth > 992) closeSidebar(); });
}

/* ── Auth Guard ── */
function authGuard() {
    const loggedIn = localStorage.getItem("waffyClinicLoggedIn");
    if (loggedIn !== "true") {
        window.location.href = "AdminClinicDoctor-Login.html";
    }
}

/* ── Logout ── */
function doLogout() {
    localStorage.removeItem("waffyClinicLoggedIn");
    localStorage.removeItem("waffyClinicRole");
    window.location.href = "AdminClinicDoctor-Login.html";
}

/* ── Sidebar Profile (footer) ── */
function openSidebarProfile() {
    toast("Profile menu preview.");
}

/* ── Save Section ── */
/**
 * Generic section save handler.
 * Pass the button element and an optional endpoint hint.
 * In ASP.NET MVC: replace the toast with an actual fetch/form submit.
 *
 * @param {HTMLElement} btn  - the Save button clicked
 * @param {string} section   - identifier used for toast messaging
 */
function saveSection(btn, section) {
    if (!btn) return;
    const old = btn.textContent;
    btn.disabled = true;
    btn.textContent = "✅ Saved!";
    setTimeout(() => {
        btn.textContent = old;
        btn.disabled = false;
    }, 1800);
    toast((section || "Settings") + " saved successfully.");
}

/* ── Toggle Availability (Work Schedule page) ── */
function toggleAvail(btn) {
    if (!btn) return;
    const isAvail = btn.textContent.includes("Available");
    btn.textContent = isAvail ? "🔴 Unavailable" : "🟢 Available for booking";
    btn.classList.toggle("btn-primary", !isAvail);
    btn.classList.toggle("btn-outline", isAvail);
    toast(isAvail ? "Booking disabled." : "Booking enabled.");
}

/* ── Toggle Notification (Notifications page) ── */
function toggleNotif(btn) {
    if (!btn) return;
    const isEnabled = btn.textContent.trim() === "Enabled";
    btn.textContent = isEnabled ? "Off" : "Enabled";
    btn.classList.toggle("btn-primary", !isEnabled);
    btn.classList.toggle("btn-outline", isEnabled);
    toast("Notification preference updated.");
}

/* ── Toggle 2FA (Password page) ── */
function toggle2FA(btn) {
    if (!btn) return;
    const isEnabled = btn.textContent.trim() === "Disable";
    btn.textContent = isEnabled ? "Enable" : "Disable";
    btn.classList.toggle("btn-primary", isEnabled);
    btn.classList.toggle("btn-outline", !isEnabled);
    toast("Two-factor authentication " + (isEnabled ? "disabled." : "enabled."));
}

/* ── Day Chip Toggle ── */
function bindDayChips() {
    qsa(".day-chip").forEach(chip => {
        if (!chip.hasAttribute("onclick")) {
            chip.addEventListener("click", () => {
                chip.classList.toggle("on");
            });
        }
    });
}

/* ── Real Image Upload ── */
function attachRealImageUploads() {
    qsa('input[type="file"]').forEach(input => {
        if (input.dataset.boundUpload === "true") return;
        input.dataset.boundUpload = "true";
        input.addEventListener("change", () => {
            const file = input.files && input.files[0];
            if (!file) return;
            if (!file.type.startsWith("image/")) { toast("Please choose a valid image file.", "error"); input.value = ""; return; }
            if (file.size > 5 * 1024 * 1024) { toast("Image is too large. Max 5MB.", "error"); input.value = ""; return; }
            const reader = new FileReader();
            reader.onload = e => {
                const dataUrl = e.target.result;
                input.dataset.imageData = dataUrl;
                const wrapper = input.closest(".f-group") || input.parentElement;
                if (!wrapper) { toast("Image uploaded successfully."); return; }
                let previewWrap = qs(".preview-wrap", wrapper);
                let previewImg = qs(".preview-img", wrapper);
                if (!previewWrap) {
                    previewWrap = document.createElement("div");
                    previewWrap.className = "preview-wrap";
                    previewWrap.style.cssText = "display:block;margin-top:10px;";
                    wrapper.appendChild(previewWrap);
                }
                if (!previewImg) {
                    previewImg = document.createElement("img");
                    previewImg.className = "preview-img";
                    previewImg.style.cssText = "width:82px;height:82px;object-fit:cover;border-radius:16px;border:1px solid rgba(151,173,77,.28);";
                    previewWrap.appendChild(previewImg);
                }
                previewWrap.style.display = "block";
                previewImg.src = dataUrl;
                previewImg.alt = file.name;
                toast("Image uploaded successfully.");
            };
            reader.readAsDataURL(file);
        });
    });
}

/* ── Upload Box Click-to-trigger ── */
function bindUploadBoxes() {
    qsa(".upload-box").forEach(box => {
        const input = box.querySelector('input[type="file"]') || box.closest(".f-group")?.querySelector('input[type="file"]');
        if (!input) {
            box.addEventListener("click", () => {
                const fileInput = document.createElement("input");
                fileInput.type = "file";
                fileInput.accept = "image/*,.pdf";
                fileInput.style.display = "none";
                document.body.appendChild(fileInput);
                fileInput.click();
                fileInput.addEventListener("change", () => {
                    if (fileInput.files[0]) toast("File selected: " + fileInput.files[0].name);
                    document.body.removeChild(fileInput);
                });
            });
        }
    });
}

/* ── Top notif button ── */
function bindTopNav() {
    const notifBtn = qs(".notif-btn");
    if (notifBtn) notifBtn.addEventListener("click", () => toast("You have 3 new notifications."));
    const tnProfile = qs(".tn-profile");
    if (tnProfile) tnProfile.addEventListener("click", () => toast("Profile menu preview."));
}

/* ── DOMContentLoaded ── */
document.addEventListener("DOMContentLoaded", () => {
    authGuard();
    attachRealImageUploads();
    bindSidebarAutoClose();
    bindDayChips();
    bindUploadBoxes();
    bindTopNav();
});