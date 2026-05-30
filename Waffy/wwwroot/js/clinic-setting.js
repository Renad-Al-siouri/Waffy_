/* ============================================================
   WAFFY — Clinic Settings · Shared JavaScript
   clinic-settings.js
   Used by all clinic-settings-*.html pages.
============================================================ */

/* ── Helpers ────────────────────────────────────────────── */
function byId(id) { return document.getElementById(id); }
function qs(sel, scope) { return (scope || document).querySelector(sel); }
function qsa(sel, scope) { return Array.from((scope || document).querySelectorAll(sel)); }

/* ── Toast ──────────────────────────────────────────────── */
function toast(msg, type) {
    let el = byId("toastMsg");
    if (!el) {
        el = document.createElement("div");
        el.id = "toastMsg";
        el.className = "toast-msg";
        document.body.appendChild(el);
    }
    el.textContent = msg;
    if (type === "error") {
        el.style.background = "linear-gradient(180deg,#d98b83 0%,#c86a5c 100%)";
    } else if (type === "warning") {
        el.style.background = "linear-gradient(180deg,#d8a74e 0%,#bf7a23 100%)";
    } else {
        el.style.background = "linear-gradient(180deg,#9ab451 0%,#7d953d 100%)";
    }
    el.classList.add("show");
    clearTimeout(el._t);
    el._t = setTimeout(() => el.classList.remove("show"), 2200);
}

/* ── Sidebar ────────────────────────────────────────────── */
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
function openSidebarProfile() { toast("Profile menu preview."); }

function bindSidebarAutoClose() {
    qsa(".wa-sb .sb-row").forEach(item => {
        item.addEventListener("click", () => {
            if (window.innerWidth <= 992) closeSidebar();
        });
    });
    window.addEventListener("resize", () => {
        if (window.innerWidth > 992) closeSidebar();
    });
}

/* ── Auth ───────────────────────────────────────────────── */
function doLogout() {
    window.location.href = "/ClinicAdmin/Logout";
}

/* ── Save feedback helper ───────────────────────────────── */
function saveWithFeedback(btn, successMsg) {
    if (!btn) {
        // called without a button reference (e.g. onclick="saveSection()")
        btn = qs(".btn-primary");
    }
    if (!btn) { toast(successMsg || "Changes saved."); return; }
    const old = btn.textContent;
    btn.textContent = "✅ Saved!";
    setTimeout(() => { btn.textContent = old; }, 1800);
    toast(successMsg || "Changes saved.");
}

/* ── Tab navigation between pages ──────────────────────── */
function highlightActiveTab() {
    const path = window.location.pathname.split("/").pop().toLowerCase();
    qsa(".cs-tab-nav a").forEach(a => {
        const href = (a.getAttribute("href") || "").toLowerCase();
        a.classList.toggle("active", path === href || path.includes(href.replace(".html", "")));
    });
}

/* ── Day-chip keyboard / toggle ─────────────────────────── */
function bindDayChips() {
    qsa(".day-chip").forEach(chip => {
        if (!chip.hasAttribute("onclick")) {
            chip.addEventListener("click", () => chip.classList.toggle("on"));
        }
    });
}

/* ── Real image upload preview ───────────────────────────── */
function attachRealImageUploads() {
    qsa('input[type="file"]').forEach(input => {
        if (input.dataset.boundUpload === "true") return;
        input.dataset.boundUpload = "true";
        input.addEventListener("change", () => {
            const file = input.files && input.files[0];
            if (!file) return;
            if (!file.type.startsWith("image/")) {
                toast("Please choose a valid image file.", "error");
                input.value = ""; return;
            }
            if (file.size > 5 * 1024 * 1024) {
                toast("Image is too large. Max 5MB.", "error");
                input.value = ""; return;
            }
            const reader = new FileReader();
            reader.onload = e => {
                const dataUrl = e.target.result;
                const wrapper = input.closest(".f-group") || input.parentElement;
                if (!wrapper) { toast("Image uploaded successfully."); return; }
                let previewWrap = wrapper.querySelector(".preview-wrap");
                let previewImg = wrapper.querySelector(".preview-img");
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

/* ── Bootstrap ──────────────────────────────────────────── */
document.addEventListener("DOMContentLoaded", () => {
    // Auth guard


    bindSidebarAutoClose();
    bindDayChips();
    attachRealImageUploads();
    highlightActiveTab();

    // Topnav notifications
    const notifBtn = qs(".notif-btn");
    if (notifBtn) notifBtn.onclick = () => toast("You have 3 new notifications.");
});
