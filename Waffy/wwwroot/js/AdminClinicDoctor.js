/* =========================================================
   WAFFY — Admin Clinic & Doctor
   Final Multi-page JS — ASP.NET Core MVC Version
========================================================= */

let adoptionViewMode = "cards";
let adoptionMode = "clinic";
let currentRole = "manager";
let chartsInitialized = false;
let chartInstances = [];
let clinicMap = null;
let clinicMarker = null;

/* =========================
   Adoption Data
========================= */
let adoptionPets = [
    {
        id: 1,
        name: "Mishmish",
        type: "cat",
        typeLabel: "Cat",
        breed: "Persian Mix",
        age: "2 years",
        gender: "Female",
        color: "Cream",
        health: "Healthy",
        vacc: "Yes",
        neuter: "Yes",
        vaccDate: "2026-03-01",
        problems: "",
        traits: "Friendly, calm, loving",
        energy: "Low",
        children: "Yes",
        otherPets: "Yes",
        approvalStatus: "approved",
        adoptionStatus: "available",
        desc: "A rescued cat with a calm and affectionate personality.",
        image: ""
    },
    {
        id: 2,
        name: "Rocky",
        type: "dog",
        typeLabel: "Dog",
        breed: "Mixed",
        age: "1 year",
        gender: "Male",
        color: "Brown",
        health: "Has health issues",
        vacc: "Yes",
        neuter: "Yes",
        vaccDate: "2026-02-14",
        problems: "Needs special care",
        traits: "Loyal, calm",
        energy: "Medium",
        children: "No",
        otherPets: "Yes",
        approvalStatus: "approved",
        adoptionStatus: "reserved",
        desc: "A sweet dog that needs a caring adopter.",
        image: ""
    },
    {
        id: 3,
        name: "Bebe",
        type: "bird",
        typeLabel: "Bird",
        breed: "Cockatiel",
        age: "6 months",
        gender: "Female",
        color: "Yellow",
        health: "Healthy",
        vacc: "No",
        neuter: "—",
        vaccDate: "",
        problems: "",
        traits: "Quiet, friendly",
        energy: "Low",
        children: "Yes",
        otherPets: "Yes",
        approvalStatus: "pending",
        adoptionStatus: "available",
        desc: "A lovely bird with a gentle temperament.",
        image: ""
    },
    {
        id: 4,
        name: "Lulu",
        type: "cat",
        typeLabel: "Cat",
        breed: "Local Mix",
        age: "3 years",
        gender: "Female",
        color: "White",
        health: "Healthy",
        vacc: "Yes",
        neuter: "Yes",
        vaccDate: "2026-01-20",
        problems: "",
        traits: "Friendly, playful",
        energy: "Medium",
        children: "Yes",
        otherPets: "Yes",
        approvalStatus: "approved",
        adoptionStatus: "adopted",
        desc: "Very social and playful cat.",
        image: ""
    }
];

const adoptionTypeIcon = {
    cat: "🐈",
    dog: "🐕",
    bird: "🐦",
    other: "🐾"
};

const adoptionApprovalMap = {
    approved: { label: "Published", cls: "s-confirmed" },
    pending: { label: "Pending", cls: "s-pending" },
    rejected: { label: "Rejected", cls: "s-cancelled" }
};

const adoptionStatusMap = {
    available: { label: "Available", cls: "s-new" },
    reserved: { label: "Reserved", cls: "s-pending" },
    adopted: { label: "Adopted", cls: "s-confirmed" }
};

/* =========================
   Helpers
========================= */
function byId(id) {
    return document.getElementById(id);
}

function qs(selector, scope = document) {
    return scope.querySelector(selector);
}

function qsa(selector, scope = document) {
    return Array.from(scope.querySelectorAll(selector));
}

function safeSetText(id, value) {
    const el = byId(id);
    if (el) el.textContent = value;
}

function safeSetHTML(id, value) {
    const el = byId(id);
    if (el) el.innerHTML = value;
}

function toggleDisplay(id, value) {
    const el = byId(id);
    if (el) el.style.display = value;
}

function addClass(id, className) {
    const el = byId(id);
    if (el) el.classList.add(className);
}

function removeClass(id, className) {
    const el = byId(id);
    if (el) el.classList.remove(className);
}

function hasEl(id) {
    return !!byId(id);
}

/* ─────────────────────────────────────────────
   currentPath()
   Returns the last segment of the MVC route.
   e.g. /ClinicAdmin/Dashboard  →  "dashboard"
        /ClinicAdmin/Medical_Records  →  "medical_records"
───────────────────────────────────────────── */
function currentPath() {
    return window.location.pathname.split("/").pop().toLowerCase();
}

/* ─────────────────────────────────────────────
   Page-detection helpers
   Updated to match ASP.NET MVC action names
───────────────────────────────────────────── */
function isLoginPage() {
    return currentPath().includes("login") || hasEl("loginEmail");
}

function isDashboardPage() {
    return currentPath().includes("dashboard") || hasEl("page-dashboard");
}

function isAppointmentsPage() {
    return currentPath().includes("appointments") || hasEl("page-appointments");
}

function isPatientsPage() {
    return currentPath().includes("patients") || hasEl("page-patients");
}

function isRecordsPage() {
    // Action name: Medical_Records
    return currentPath().includes("medical_records") || hasEl("page-records");
}

function isDoctorsPage() {
    return currentPath().includes("doctors") || hasEl("page-doctors");
}

function isSettingsPage() {
    // Action name: Clinic_Settings  (NOT My_Settings which is doctor-only)
    return currentPath().includes("clinic_settings") || hasEl("page-clinic");
}

function isRevenuePage() {
    return currentPath().includes("revenue") || hasEl("page-revenue") || hasEl("tab-rev-payments");
}

function isReviewsPage() {
    return currentPath().includes("reviews") || hasEl("page-reviews");
}

function isReportsPage() {
    return currentPath().includes("reports") || hasEl("page-reports");
}

function isAdoptionPage() {
    return currentPath().includes("adoption") || hasEl("page-adoption") || hasEl("adoptionCardsGrid");
}

function isDoctorSettingsPage() {
    // Action name: My_Settings
    return currentPath().includes("my_settings") || hasEl("page-doctor-settings");
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

/* =========================
   Toast
========================= */
function toast(msg, type = "success") {
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

/* =========================
   Login / Auth / Role
========================= */
function selectRole(role) {
    currentRole = role;

    const roleManager = byId("roleManager");
    const roleDoctor = byId("roleDoctor");

    if (roleManager) roleManager.classList.toggle("selected", role === "manager");
    if (roleDoctor) roleDoctor.classList.toggle("selected", role === "doctor");
}

function doLogin() {
    const email = byId("loginEmail")?.value.trim();
    const password = byId("loginPassword")?.value.trim();

    if (!email || !password) {
        alert("Please enter your email and password.");
        return;
    }

    localStorage.setItem("waffyClinicLoggedIn", "true");
    localStorage.setItem("waffyClinicRole", currentRole);

    // ── MVC route ──
    window.location.href = "/ClinicAdmin/Dashboard";
}

function doLogout() {
    localStorage.removeItem("waffyClinicLoggedIn");
    localStorage.removeItem("waffyClinicRole");
    sessionStorage.removeItem("waffy_record_prefill");

    // ── MVC route ──
    window.location.href = "/ClinicAdmin/Login";
}

function openSidebarProfile() {
    toast("Profile menu preview.");
}

function applyRole() {
    const isMgr = currentRole === "manager";

    const roleTag = byId("roleTag");
    const userAva = byId("userAva");
    const userName = byId("userName");
    const subBanner = byId("subBanner");
    const subCard = byId("subCard");
    const managerSection = byId("managerSection");
    const doctorSettingsSection = byId("doctorSettingsSection");
    const todayTitle = byId("todayTitle");
    const todayStats = byId("todayStats");
    const kpiGrid = byId("kpiGrid");
    const doctorFilter = byId("doctorFilter");

    if (roleTag) {
        roleTag.textContent = isMgr ? "Clinic Manager 👑" : "Dr. Ahmad Al-Zoubi 👨‍⚕️";
        roleTag.className = "role-tag " + (isMgr ? "manager" : "doctor");
    }

    if (userAva) userAva.textContent = isMgr ? "M" : "A";
    if (userName) userName.textContent = isMgr ? "Clinic Manager" : "Dr. Ahmad Al-Zoubi";

    const brandTag = qs(".sb-wm-tag");
    const identityPill = byId("tnIdentityPill");

    if (brandTag) brandTag.textContent = isMgr ? "Clinic Admin" : "Doctor Panel";
    if (identityPill) identityPill.textContent = isMgr ? "🏥 Al Amanah Veterinary Clinic" : "👨‍⚕️ Dr. Ahmad Al-Zoubi";

    syncTopProfile();
    normalizeHeroActions();

    if (subBanner) subBanner.style.display = isMgr ? "" : "none";
    if (subCard) subCard.style.display = isMgr ? "" : "none";

    if (managerSection) managerSection.classList.toggle("sb-role-hidden", !isMgr);
    if (doctorSettingsSection) doctorSettingsSection.classList.toggle("sb-role-hidden", isMgr);

    if (!isMgr) {
        if (todayTitle) todayTitle.textContent = "☀️ Good morning, Dr. Ahmad!";
        if (todayStats) {
            todayStats.innerHTML = `
        <div class="today-stat"><div class="today-stat-val">6</div><div class="today-stat-label">My appointments</div></div>
        <div class="today-stat"><div class="today-stat-val">2</div><div class="today-stat-label">New patients</div></div>
        <div class="today-stat"><div class="today-stat-val">4.9</div><div class="today-stat-label">My rating ⭐</div></div>
      `;
        }

        if (kpiGrid && isDashboardPage()) {
            kpiGrid.innerHTML = `
        <div class="kpi-card green">
          <div class="kpi-top"><div class="kpi-icon green">📅</div><span class="kpi-badge badge-up">↑ 5%</span></div>
          <div class="kpi-val">142</div>
          <div class="kpi-label">My appointments this month</div>
        </div>
        <div class="kpi-card blue">
          <div class="kpi-top"><div class="kpi-icon blue">🐾</div></div>
          <div class="kpi-val">18</div>
          <div class="kpi-label">New patients</div>
        </div>
        <div class="kpi-card orange">
          <div class="kpi-top"><div class="kpi-icon orange">⭐</div></div>
          <div class="kpi-val">4.9</div>
          <div class="kpi-label">My rating</div>
        </div>
        <div class="kpi-card green">
          <div class="kpi-top"><div class="kpi-icon green">💰</div></div>
          <div class="kpi-val">385</div>
          <div class="kpi-label">My revenue (JOD)</div>
        </div>
      `;
            kpiGrid.className = "kpi-grid kpi-4";
        }

        if (doctorFilter) doctorFilter.style.display = "none";
    } else {
        if (doctorFilter) doctorFilter.style.display = "";
    }
}

/* =========================
   Save Actions
========================= */
function saveClinic(btn) {
    if (!btn) return;
    const old = btn.textContent;
    btn.textContent = "✅ Saved!";
    setTimeout(() => { btn.textContent = old; }, 1800);
    toast("Clinic settings saved.");
}

function saveDoctor(btn) {
    if (!btn) return;
    const old = btn.textContent;
    btn.textContent = "✅ Saved!";
    setTimeout(() => { btn.textContent = old; }, 1800);
    toast("Doctor settings saved.");
}

/* =========================
   Modal
========================= */
function openModal(id) {
    addClass(id, "open");
    addClass(id, "on");
}

function closeModal(id) {
    removeClass(id, "open");
    removeClass(id, "on");
}

function closeOnBg(e, id) {
    const modal = byId(id);
    if (modal && e.target === modal) closeModal(id);
}

/* =========================
   Tabs
========================= */
function switchTab(btn, id) {
    if (!btn) return;

    const parent =
        btn.closest(".page-content") ||
        btn.closest(".page-wrap") ||
        btn.closest(".content-area") ||
        document.body;

    qsa(".tab", parent).forEach(t => t.classList.remove("active"));
    btn.classList.add("active");

    const sectionPrefix = id.split("-")[0];
    qsa(`[id^="tab-${sectionPrefix}-"]`, parent).forEach(tabPane => {
        tabPane.style.display = "none";
    });

    const target = byId("tab-" + id);
    if (target) target.style.display = "";
}

/* =========================
   Charts
========================= */
if (typeof Chart !== "undefined") {
    Chart.defaults.font.family = "Nunito";
    Chart.defaults.color = "#7b6c58";
    Chart.defaults.font.size = 11;
    Chart.defaults.borderColor = "rgba(151,173,77,0.12)";
}

const gc = "rgba(151,173,77,0.14)";
const m8 = ["Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar"];

function resizeAllCharts() {
    if (!chartInstances.length) return;
    chartInstances.forEach(chart => { try { chart.resize(); } catch (e) { } });
}

function initCharts() {
    if (chartsInitialized || typeof Chart === "undefined") return;

    const apptChart = byId("apptChart");
    const servicesChart = byId("servicesChart");
    const revenueChart = byId("revenueChart");
    const reportsChart = byId("reportsChart");

    if (apptChart) {
        chartInstances.push(new Chart(apptChart, {
            type: "bar",
            data: {
                labels: m8,
                datasets: [
                    {
                        label: "Booked",
                        data: [420, 450, 480, 510, 540, 570, 610, 638],
                        backgroundColor: "rgba(125,149,61,0.82)",
                        borderRadius: 5
                    },
                    {
                        label: "No-show",
                        data: [95, 98, 94, 96, 98, 100, 105, 77],
                        backgroundColor: "rgba(200,106,92,0.72)",
                        borderRadius: 5
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { x: { grid: { color: gc } }, y: { grid: { color: gc } } }
            }
        }));
    }

    if (servicesChart) {
        chartInstances.push(new Chart(servicesChart, {
            type: "doughnut",
            data: {
                labels: ["Checkup", "Vaccination", "Surgery"],
                datasets: [{ data: [42, 31, 27], backgroundColor: ["#7d953d", "#bf7a23", "#8aa14a"], borderWidth: 0 }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                cutout: "65%"
            }
        }));
    }

    if (revenueChart) {
        chartInstances.push(new Chart(revenueChart, {
            type: "line",
            data: {
                labels: m8,
                datasets: [{
                    data: [580, 620, 660, 700, 740, 780, 810, 840],
                    borderColor: "#8aa14a",
                    backgroundColor: "rgba(138,161,74,0.10)",
                    tension: 0.4,
                    fill: true,
                    pointRadius: 4,
                    pointBackgroundColor: "#8aa14a"
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { x: { grid: { color: gc } }, y: { grid: { color: gc } } }
            }
        }));
    }

    if (reportsChart) {
        chartInstances.push(new Chart(reportsChart, {
            type: "bar",
            data: {
                labels: m8,
                datasets: [{
                    data: [420, 450, 480, 510, 540, 570, 610, 638],
                    backgroundColor: "rgba(125,149,61,0.80)",
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { x: { grid: { color: gc } }, y: { grid: { color: gc } } }
            }
        }));
    }

    chartsInitialized = true;
    resizeAllCharts();
}

/* =========================
   Search / Filters / Data Setup
========================= */
const pageState = {
    appointmentRange: "today",
    patientVaccination: "all"
};

function parseSpecies(text) {
    const t = (text || "").toLowerCase();
    if (t.includes("dog")) return "dogs";
    if (t.includes("cat")) return "cats";
    if (t.includes("bird")) return "birds";
    if (t.includes("rabbit")) return "rabbits";
    return "other";
}

function setupData() {
    if (isAppointmentsPage()) {
        const apptRows = qsa("tbody tr");
        const ranges = ["today", "today", "week"];
        apptRows.forEach((tr, i) => {
            if (tr.classList.contains("empty-row")) return;
            tr.dataset.range = ranges[i] || "month";
            tr.dataset.search = tr.innerText.toLowerCase();
            tr.dataset.doctor = tr.children[2]?.innerText.trim() || "";
            tr.dataset.status = (tr.children[5]?.innerText || "").trim().toLowerCase();
            tr.dataset.pet = (tr.children[1]?.innerText.split("—")[0] || "").replace(/[^\p{L}\p{N}\s]/gu, "").trim();
        });
    }

    if (isPatientsPage()) {
        const patientRows = qsa("tbody tr");
        patientRows.forEach(tr => {
            if (tr.classList.contains("empty-row")) return;
            const typeText = tr.children[1]?.innerText || "";
            const vaccText = tr.children[5]?.innerText.toLowerCase() || "";
            tr.dataset.species = parseSpecies(typeText);
            tr.dataset.vaccination = vaccText.includes("expired") ? "expired" : vaccText.includes("due") ? "due" : "updated";
            tr.dataset.search = tr.innerText.toLowerCase();
            tr.dataset.pet = (tr.children[0]?.innerText || "").replace(/[^\p{L}\p{N}\s]/gu, "").trim();
        });
    }

    if (isRecordsPage()) {
        const recordItems = qsa(".record-item");
        recordItems.forEach(item => {
            const txt = item.innerText.toLowerCase();
            item.dataset.search = txt;
            item.dataset.type = txt.includes("vaccination") ? "vaccination" : txt.includes("surgery") ? "surgery" : "checkup";
            item.dataset.pet = (qs(".record-title", item)?.innerText.split("—")[0] || "").replace(/[^\p{L}\p{N}\s]/gu, "").trim();
        });
    }

    if (isDoctorsPage()) {
        const doctorCards = qsa(".doctor-card");
        doctorCards.forEach(card => {
            const name = qs(".doctor-name", card)?.innerText || "";
            if (name) {
                card.dataset.name = name;
                card.dataset.search = card.innerText.toLowerCase();
                card.dataset.active = card.innerText.toLowerCase().includes("active") ? "active" : "other";
            }
        });
    }

    if (isRevenuePage()) {
        const paymentRows = qsa("#tab-rev-payments tbody tr");
        paymentRows.forEach(tr => {
            if (tr.classList.contains("empty-row")) return;
            tr.dataset.type = tr.children[2]?.innerText.trim().toLowerCase() || "";
            tr.dataset.status = tr.children[6]?.innerText.trim().toLowerCase() || "";
            tr.dataset.search = tr.innerText.toLowerCase();
        });
    }
}

function ensureEmpty(tbodySelector, visible, colspan, msg = "No matching rows found.") {
    const tb = qs(tbodySelector);
    if (!tb) return;

    let empty = qs(".empty-row", tb);
    if (!empty) {
        empty = document.createElement("tr");
        empty.className = "empty-row row-hidden";
        empty.innerHTML = `<td colspan="${colspan}"><div class="empty-msg">${msg}</div></td>`;
        tb.appendChild(empty);
    }

    empty.classList.toggle("row-hidden", visible !== 0);
}

function filterAppointments() {
    if (!isAppointmentsPage()) return;

    const search = qs(".search-input input")?.value.trim().toLowerCase() || "";
    const filterBar = qs(".filter-bar");
    const selects = filterBar ? qsa("select", filterBar) : [];
    const doctor = (selects[0]?.value || "All Doctors").toLowerCase();
    const status = (selects[1]?.value || "All statuses").toLowerCase();

    let visible = 0;

    qsa("tbody tr").forEach(tr => {
        if (tr.classList.contains("empty-row")) return;

        const okSearch = !search || (tr.dataset.search || "").includes(search);
        const okDoctor = doctor.includes("all") || (tr.dataset.doctor || "").toLowerCase() === doctor;
        const okStatus = status.includes("all") || (tr.dataset.status || "").includes(status.replace(" statuses", "").trim());
        const okRange = pageState.appointmentRange === "month"
            ? true
            : (tr.dataset.range === pageState.appointmentRange ||
                (pageState.appointmentRange === "week" && tr.dataset.range === "today"));

        const show = okSearch && okDoctor && okStatus && okRange;
        tr.classList.toggle("row-hidden", !show);
        if (show) visible++;
    });

    ensureEmpty("tbody", visible, 7);
}

function filterPatients() {
    if (!isPatientsPage()) return;

    const search = qs(".search-input input")?.value.trim().toLowerCase() || "";
    const species = qs(".filter-bar select")?.value.toLowerCase() || "all types";

    let visible = 0;

    qsa("tbody tr").forEach(tr => {
        if (tr.classList.contains("empty-row")) return;

        const okSearch = !search || (tr.dataset.search || "").includes(search);
        const okSpecies = species.includes("all") || tr.dataset.species === species;
        const okVacc = pageState.patientVaccination === "all" || tr.dataset.vaccination === pageState.patientVaccination;

        const show = okSearch && okSpecies && okVacc;
        tr.classList.toggle("row-hidden", !show);
        if (show) visible++;
    });

    ensureEmpty("tbody", visible, 7);
}

function filterRecords(prefill = "") {
    if (!isRecordsPage()) return;

    const searchInput = qs(".search-input input");
    if (searchInput && prefill) searchInput.value = prefill;

    const search = searchInput?.value.trim().toLowerCase() || "";
    const type = qs(".filter-bar select")?.value.toLowerCase() || "all types";

    let visible = 0;

    qsa(".record-item").forEach(item => {
        const okSearch = !search || (item.dataset.search || "").includes(search);
        const okType = type.includes("all") || item.dataset.type === type;
        const show = okSearch && okType;

        item.classList.toggle("record-hidden", !show);
        if (show) visible++;
    });

    let empty = qs(".records-empty");
    if (!empty) {
        empty = document.createElement("div");
        empty.className = "empty-msg records-empty";
        document.body.appendChild(empty);
    }

    empty.style.display = visible ? "none" : "block";
    empty.textContent = "No medical records match your filters.";
}

function filterPayments() {
    if (!isRevenuePage()) return;

    const tab = byId("tab-rev-payments");
    if (!tab) return;

    const selects = qsa("select", tab);
    const type = (selects[0]?.value || "All types").toLowerCase();
    const status = (selects[1]?.value || "All statuses").toLowerCase();

    let visible = 0;

    qsa("tbody tr", tab).forEach(tr => {
        if (tr.classList.contains("empty-row")) return;

        const okType = type.includes("all") || (tr.dataset.type || "").includes(type);
        const okStatus = status.includes("all") || (tr.dataset.status || "").includes(status);
        const show = okType && okStatus;

        tr.classList.toggle("row-hidden", !show);
        if (show) visible++;
    });

    ensureEmpty("#tab-rev-payments tbody", visible, 7, "No payments match your filters.");
}

/* =========================
   Buttons / Actions
========================= */
function bindAppointmentRow(tr) {
    const buttons = qsa("button", tr);
    buttons.forEach(btn => {
        if (btn.textContent.includes("Accept")) {
            btn.onclick = () => {
                tr.dataset.status = "confirmed";
                const cell = tr.children[5];
                if (cell) cell.innerHTML = '<span class="status s-confirmed">Confirmed</span>';
                if (tr.children[6]) {
                    tr.children[6].innerHTML = '<div style="display:flex;gap:4px"><button class="btn btn-primary btn-sm">Details</button><button class="btn btn-outline btn-sm">✏️</button></div>';
                }
                bindAppointmentRow(tr);
                filterAppointments();
                toast("Appointment confirmed.");
            };
        } else if (btn.textContent.includes("Decline")) {
            btn.onclick = () => {
                tr.dataset.status = "cancelled";
                if (tr.children[5]) tr.children[5].innerHTML = '<span class="status s-cancelled">Cancelled</span>';
                filterAppointments();
                toast("Appointment declined.");
            };
        } else if (btn.textContent.includes("Details")) {
            btn.onclick = () => toast("Opening appointment details...");
        } else if (btn.textContent.includes("✏️")) {
            btn.onclick = () => openModal("modalAppt");
        }
    });
}

function bindPatientRow(tr) {
    const [recordBtn, editBtn] = qsa("button", tr);
    if (recordBtn) {
        recordBtn.onclick = () => {
            sessionStorage.setItem("waffy_record_prefill", (tr.dataset.pet || "").toLowerCase());
            // ── MVC route ──
            window.location.href = "/ClinicAdmin/Medical_Records";
        };
    }
    if (editBtn) editBtn.onclick = () => openModal("modalPet");
}

function bindRecordItem(item) {
    const [editBtn, delBtn] = qsa("button", item);
    if (editBtn) editBtn.onclick = () => openModal("modalRecord");
    if (delBtn) {
        delBtn.onclick = () => {
            item.remove();
            filterRecords();
            toast("Medical record removed.");
        };
    }
}

function bindDoctorCard(card) {
    const editBtn = qs("button", card);
    if (editBtn) {
        editBtn.onclick = e => {
            e.stopPropagation();
            openModal("modalDoctorEdit");
        };
    }
}

function exportTable(rootSelector, filename) {
    const root = qs(rootSelector);
    if (!root) return;

    const rows = qsa("table tr", root).filter(r => !r.classList.contains("row-hidden"));
    const csv = rows.map(r =>
        qsa("th,td", r).map(c => `"${c.innerText.replace(/"/g, '""')}"`).join(",")
    ).join("\n");

    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    a.click();
    URL.revokeObjectURL(a.href);
    toast("Export started.");
}

function addAppointment() {
    const modal = byId("modalAppt");
    if (!modal) return;

    const vals = qsa("input, select, textarea", modal).map(el => el.value.trim());
    const [pet, owner, doctor, service, date, time, amount] = vals;

    if (!pet || !owner || !service) {
        toast("Please fill the required appointment fields.", "warning");
        return;
    }

    const tbody = qs("tbody");
    if (!tbody) return;

    const tr = document.createElement("tr");
    tr.innerHTML = `
    <td><b>${time || "12:00 PM"}</b></td>
    <td>${pet} — ${owner}</td>
    <td>${doctor || "Dr. Ahmad Al-Zoubi"}</td>
    <td>${service}</td>
    <td style="color:var(--gd)">✅ ${amount || "0"} JOD</td>
    <td><span class="status s-confirmed">Confirmed</span></td>
    <td><div style="display:flex;gap:4px"><button class="btn btn-primary btn-sm">Details</button><button class="btn btn-outline btn-sm">✏️</button></div></td>
  `;
    tbody.prepend(tr);

    tr.dataset.range = "today";
    tr.dataset.search = tr.innerText.toLowerCase();
    tr.dataset.doctor = doctor || "Dr. Ahmad Al-Zoubi";
    tr.dataset.status = "confirmed";
    tr.dataset.pet = pet.replace(/[^\p{L}\p{N}\s]/gu, "").trim();

    bindAppointmentRow(tr);

    qsa("input, select, textarea", modal).forEach(el => {
        if (el.tagName === "SELECT") el.selectedIndex = 0;
        else el.value = "";
    });

    closeModal("modalAppt");
    filterAppointments();
    toast("Appointment added successfully.");
}

function addPatient() {
    const modal = byId("modalPet");
    if (!modal) return;

    const inputs = qsa("input, select", modal);
    if (!inputs.length) return;

    const pet = inputs[0]?.value.trim();
    const type = inputs[1]?.value.trim() || "Dog";
    const breed = inputs[2]?.value.trim() || "Mixed";
    const age = inputs[3]?.value.trim() || "1 year";
    const gender = inputs[4]?.value.trim() || "Male";
    const owner = inputs[7]?.value.trim();

    if (!pet || !owner) {
        toast("Please enter the pet name and owner name.", "warning");
        return;
    }

    const emoji = type.toLowerCase().includes("cat") ? "🐈"
        : type.toLowerCase().includes("rabbit") ? "🐇"
            : type.toLowerCase().includes("bird") ? "🦜"
                : "🐕";

    const tbody = qs("tbody");
    if (!tbody) return;

    const tr = document.createElement("tr");
    tr.innerHTML = `
    <td>${emoji} <b>${pet}</b></td>
    <td>${type} — ${breed}</td>
    <td>${age} — ${gender}</td>
    <td>${owner}</td>
    <td>${new Date().toLocaleDateString("en-GB", { day: "numeric", month: "long", year: "numeric" })}</td>
    <td><span class="status s-pending">Due soon ⚠</span></td>
    <td><div style="display:flex;gap:4px"><button class="btn btn-outline btn-sm">Record</button><button class="btn btn-outline btn-sm">✏️</button></div></td>
  `;
    tbody.prepend(tr);

    tr.dataset.species = parseSpecies(type);
    tr.dataset.vaccination = "due";
    tr.dataset.search = tr.innerText.toLowerCase();
    tr.dataset.pet = pet;

    bindPatientRow(tr);

    inputs.forEach(el => {
        if (el.tagName === "SELECT") el.selectedIndex = 0;
        else el.value = "";
    });

    closeModal("modalPet");
    filterPatients();
    toast("Patient added successfully.");
}

function addRecord() {
    const modal = byId("modalRecord");
    if (!modal) return;

    const pet = qsa("select", modal)[0]?.value.replace("Choose a pet...", "").trim() || "Max";
    const doctor = qsa("select", modal)[1]?.value.trim() || "Dr. Ahmad Al-Zoubi";
    const date = qs('input[type="date"]', modal)?.value || new Date().toISOString().slice(0, 10);
    const diagnosis = qsa("textarea", modal)[0]?.value.trim() || "New diagnosis entry.";
    const treatment = qsa("textarea", modal)[2]?.value.trim() || "Treatment to be updated.";

    const filterBar = qs(".filter-bar");
    if (!filterBar) return;

    const item = document.createElement("div");
    item.className = "record-item";
    item.innerHTML = `
    <div class="record-header">
      <div>
        <div class="record-title">${pet} — Follow-up visit</div>
        <div class="record-date">${new Date(date).toLocaleDateString("en-GB", { day: "numeric", month: "long", year: "numeric" })} • ${doctor}</div>
      </div>
      <div style="display:flex;gap:6px">
        <button class="btn btn-outline btn-sm">✏️ Edit</button>
        <button class="btn btn-red btn-sm">🗑</button>
      </div>
    </div>
    <div style="font-size:.84rem;color:var(--textm)">Diagnosis: ${diagnosis}. Treatment: ${treatment}</div>
    <div class="record-tags">
      <span class="record-tag">Checkup</span>
      <span class="record-tag rx">Treatment updated</span>
    </div>
  `;

    filterBar.insertAdjacentElement("afterend", item);

    item.dataset.search = item.innerText.toLowerCase();
    item.dataset.type = "checkup";
    item.dataset.pet = pet.replace(/[^\p{L}\p{N}\s]/gu, "").trim();

    bindRecordItem(item);

    qsa("input, textarea", modal).forEach(el => el.value = "");
    qsa("select", modal).forEach(el => el.selectedIndex = 0);

    closeModal("modalRecord");
    filterRecords();
    toast("Medical record saved.");
}

function addDoctor() {
    const modal = byId("modalDoctor");
    if (!modal) return;

    const inputs = qsa("input, select, textarea", modal);
    const name = inputs[0]?.value.trim();
    const specialty = inputs[3]?.value.trim() || "General Veterinary Medicine";
    const exp = inputs[4]?.value.trim() || "1–3 years";

    if (!name) {
        toast("Please enter the doctor name.", "warning");
        return;
    }

    const grid = qs(".grid-3");
    if (!grid) return;

    const addCard = qsa(".doctor-card").find(c => !qs(".doctor-name", c));
    const card = document.createElement("div");
    card.className = "doctor-card";
    card.innerHTML = `
    <div class="doctor-ava" style="background:var(--gp)">👨‍⚕️</div>
    <div style="flex:1">
      <div class="doctor-name">${name}</div>
      <div class="doctor-spec">${specialty} • ${exp}</div>
      <div class="doctor-stats">
        <div class="doctor-stat">Appointments: <span>0</span></div>
        <div class="doctor-stat">Rating: <span>New</span></div>
      </div>
    </div>
    <div class="doctor-actions">
      <button class="btn btn-outline btn-sm">✏️</button>
      <span class="status s-pending">Pending</span>
    </div>
  `;

    grid.insertBefore(card, addCard || null);

    card.dataset.name = name;
    card.dataset.search = card.innerText.toLowerCase();
    card.dataset.active = "other";

    bindDoctorCard(card);

    qsa("input, select, textarea", modal).forEach(el => {
        if (el.tagName === "SELECT") el.selectedIndex = 0;
        else el.value = "";
    });

    closeModal("modalDoctor");
    toast("Doctor added successfully.");
}

/* =========================
   Real Image Upload
========================= */
function attachRealImageUploads() {
    qsa('input[type="file"]').forEach(input => {
        if (input.dataset.boundUpload === "true") return;
        input.dataset.boundUpload = "true";

        input.addEventListener("change", () => {
            const file = input.files && input.files[0];
            if (!file) return;

            if (!file.type.startsWith("image/")) {
                toast("Please choose a valid image file.", "error");
                input.value = "";
                return;
            }

            if (file.size > 5 * 1024 * 1024) {
                toast("Image is too large. Max 5MB.", "error");
                input.value = "";
                return;
            }

            const reader = new FileReader();
            reader.onload = e => {
                const dataUrl = e.target.result;
                input.dataset.imageData = dataUrl;

                const wrapper = input.closest(".f") || input.closest(".f-group") || input.parentElement;
                if (!wrapper) { toast("Image uploaded successfully."); return; }

                let previewWrap = qs(".preview-wrap", wrapper);
                let previewImg = qs(".preview-img", wrapper);

                if (!previewWrap) {
                    previewWrap = document.createElement("div");
                    previewWrap.className = "preview-wrap";
                    previewWrap.style.display = "block";
                    previewWrap.style.marginTop = "10px";
                    wrapper.appendChild(previewWrap);
                }

                if (!previewImg) {
                    previewImg = document.createElement("img");
                    previewImg.className = "preview-img";
                    previewImg.style.width = "82px";
                    previewImg.style.height = "82px";
                    previewImg.style.objectFit = "cover";
                    previewImg.style.borderRadius = "16px";
                    previewImg.style.border = "1px solid rgba(151,173,77,.28)";
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

/* =========================
   Leaflet Map
========================= */
function initClinicMap() {
    if (typeof L === "undefined") return;

    const mapEl = byId("map") || byId("clinicMap") || byId("petFoundMap") || byId("locationMap");
    if (!mapEl || clinicMap) return;

    let lat = parseFloat(byId("latitude")?.value || "31.9539");
    let lng = parseFloat(byId("longitude")?.value || "35.9106");

    clinicMap = L.map(mapEl).setView([lat, lng], 13);
    clinicMarker = L.marker([lat, lng], { draggable: true }).addTo(clinicMap);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: "&copy; OpenStreetMap"
    }).addTo(clinicMap);

    function updatePosition(newLat, newLng) {
        const fixedLat = Number(newLat.toFixed(6));
        const fixedLng = Number(newLng.toFixed(6));

        const latInput = byId("latitude");
        const lngInput = byId("longitude");
        const latText = byId("latText");
        const lngText = byId("lngText");

        if (latInput) latInput.value = fixedLat;
        if (lngInput) lngInput.value = fixedLng;
        if (latText) latText.textContent = fixedLat;
        if (lngText) lngText.textContent = fixedLng;
    }

    clinicMap.on("click", e => {
        clinicMarker.setLatLng(e.latlng);
        updatePosition(e.latlng.lat, e.latlng.lng);
    });

    clinicMarker.on("dragend", e => {
        const pos = e.target.getLatLng();
        updatePosition(pos.lat, pos.lng);
    });

    setTimeout(() => clinicMap.invalidateSize(), 250);
}

/* =========================
   Adoption
========================= */
function openAdoptionAddPage() {
    toggleDisplay("adoptionManagePage", "none");
    toggleDisplay("adoptionAddPage", "block");
}

function openAdoptionManagePage() {
    toggleDisplay("adoptionManagePage", "block");
    toggleDisplay("adoptionAddPage", "none");
}

function toggleAdoptionView() {
    adoptionViewMode = adoptionViewMode === "cards" ? "table" : "cards";
    toggleDisplay("adoptionCardView", adoptionViewMode === "cards" ? "block" : "none");
    toggleDisplay("adoptionTableView", adoptionViewMode === "table" ? "block" : "none");
}

function setAdoptionMode(mode) {
    adoptionMode = mode;

    const clinicBtn = byId("adoptionModeClinicBtn");
    const storeBtn = byId("adoptionModeStoreBtn");
    const manageSub = byId("adoptionManageSub");
    const addSub = byId("adoptionAddSub");

    if (clinicBtn) clinicBtn.classList.toggle("active", mode === "clinic");
    if (storeBtn) storeBtn.classList.toggle("active", mode === "store");

    if (manageSub) {
        manageSub.textContent = mode === "clinic"
            ? "Manage animals available for adoption from your clinic"
            : "Manage animals available for adoption from your store";
    }

    if (addSub) {
        addSub.textContent = mode === "clinic"
            ? "Add a pet from the clinic for adoption"
            : "Add a pet from the store for adoption";
    }
}

function getFilteredAdoptionPets() {
    const search = (byId("adoptionSearch")?.value || "").trim().toLowerCase();
    const adoptionStatus = byId("adoptionStatusFilter")?.value || "all";
    const approvalStatus = byId("adoptionApprovalFilter")?.value || "all";

    return adoptionPets.filter(p => {
        const haystack = `${p.name} ${p.typeLabel} ${p.breed} ${p.desc}`.toLowerCase();
        const matchesSearch = !search || haystack.includes(search);
        const matchesAdoption = adoptionStatus === "all" || p.adoptionStatus === adoptionStatus;
        const matchesApproval = approvalStatus === "all" || p.approvalStatus === approvalStatus;
        return matchesSearch && matchesAdoption && matchesApproval;
    });
}

function renderAdoptionStats() {
    safeSetText("adoptTotal", adoptionPets.length);
    safeSetText("adoptAvailable", adoptionPets.filter(p => p.adoptionStatus === "available").length);
    safeSetText("adoptPending", adoptionPets.filter(p => p.approvalStatus === "pending").length);
    safeSetText("adoptAdopted", adoptionPets.filter(p => p.adoptionStatus === "adopted").length);
}

function renderAdoptionCards() {
    const grid = byId("adoptionCardsGrid");
    if (!grid) return;

    const filtered = getFilteredAdoptionPets();

    if (!filtered.length) {
        grid.innerHTML = `<div class="empty-state" style="grid-column:1/-1;">No animals found.</div>`;
        return;
    }

    grid.innerHTML = filtered.map(p => `
    <div class="adoption-card" onclick="openAdoptionPetDetail(${p.id})">
      <div class="adoption-card-top">
        <div class="adoption-card-icon">${adoptionTypeIcon[p.type] || "🐾"}</div>
        <div class="adoption-card-main">
          <div class="adoption-card-name">${escapeHtml(p.name)}</div>
          <div class="adoption-card-sub">${escapeHtml(p.typeLabel)} — ${escapeHtml(p.age)} — ${escapeHtml(p.gender)}</div>
        </div>
      </div>
      <div class="adoption-card-meta">
        <span><b>Breed:</b> ${escapeHtml(p.breed)}</span>
        <span><b>Health:</b> ${escapeHtml(p.health)}</span>
      </div>
      <div class="adoption-card-badges">
        <span class="status ${adoptionStatusMap[p.adoptionStatus].cls}">${adoptionStatusMap[p.adoptionStatus].label}</span>
        <span class="status ${adoptionApprovalMap[p.approvalStatus].cls}">${adoptionApprovalMap[p.approvalStatus].label}</span>
      </div>
      <div class="adoption-card-actions">
        <button class="btn btn-outline btn-sm" onclick="event.stopPropagation(); openAdoptionPetDetail(${p.id})">View</button>
        <button class="btn btn-red btn-sm"     onclick="event.stopPropagation(); deleteAdoptionPet(${p.id})">Delete</button>
      </div>
    </div>
  `).join("");
}

function renderAdoptionTable() {
    const tbody = byId("adoptionTableBody");
    if (!tbody) return;

    const filtered = getFilteredAdoptionPets();

    if (!filtered.length) {
        tbody.innerHTML = `<tr><td colspan="8"><div class="empty-state">No animals found.</div></td></tr>`;
        return;
    }

    tbody.innerHTML = filtered.map(p => `
    <tr onclick="openAdoptionPetDetail(${p.id})" style="cursor:pointer;">
      <td><b>${escapeHtml(p.name)}</b></td>
      <td>${escapeHtml(p.typeLabel)}</td>
      <td>${escapeHtml(p.age)}</td>
      <td>${escapeHtml(p.gender)}</td>
      <td><span class="status ${p.health === "Healthy" ? "s-confirmed" : "s-pending"}">${escapeHtml(p.health)}</span></td>
      <td><span class="status ${adoptionStatusMap[p.adoptionStatus].cls}">${adoptionStatusMap[p.adoptionStatus].label}</span></td>
      <td><span class="status ${adoptionApprovalMap[p.approvalStatus].cls}">${adoptionApprovalMap[p.approvalStatus].label}</span></td>
      <td onclick="event.stopPropagation()">
        <div style="display:flex;gap:5px;flex-wrap:wrap;">
          <button class="btn btn-outline btn-sm" onclick="openAdoptionPetDetail(${p.id})">View</button>
          <button class="btn btn-red btn-sm"     onclick="deleteAdoptionPet(${p.id})">Delete</button>
        </div>
      </td>
    </tr>
  `).join("");
}

function openAdoptionPetDetail(id) {
    const p = adoptionPets.find(x => x.id === id);
    const modalTitle = byId("adoptionPetModalTitle");
    const modalBody = byId("adoptionPetModalBody");

    if (!p || !modalTitle || !modalBody) return;

    modalTitle.textContent = `🐾 ${p.name} — ${p.typeLabel}`;
    modalBody.innerHTML = `
    <div class="adoption-details-grid">
      <div class="chart-card adoption-mini-card">
        <div class="sec-label">Pet Information</div>
        <div class="adoption-detail-lines">
          <div><b>Name:</b> ${escapeHtml(p.name)}</div>
          <div><b>Type:</b> ${escapeHtml(p.typeLabel)}</div>
          <div><b>Breed:</b> ${escapeHtml(p.breed)}</div>
          <div><b>Age:</b> ${escapeHtml(p.age)}</div>
          <div><b>Gender:</b> ${escapeHtml(p.gender)}</div>
          <div><b>Color:</b> ${escapeHtml(p.color || "-")}</div>
        </div>
      </div>
      <div class="chart-card adoption-mini-card">
        <div class="sec-label">Health</div>
        <div class="adoption-detail-lines">
          <div><b>Health:</b> ${escapeHtml(p.health)}</div>
          <div><b>Vaccinated:</b> ${escapeHtml(p.vacc)}</div>
          <div><b>Neutered / Spayed:</b> ${escapeHtml(p.neuter)}</div>
          <div><b>Vaccination Date:</b> ${escapeHtml(p.vaccDate || "-")}</div>
          <div><b>Problems:</b> ${escapeHtml(p.problems || "-")}</div>
        </div>
      </div>
      <div class="chart-card adoption-mini-card">
        <div class="sec-label">Behaviour</div>
        <div class="adoption-detail-lines">
          <div><b>Traits:</b> ${escapeHtml(p.traits || "-")}</div>
          <div><b>Energy:</b> ${escapeHtml(p.energy || "-")}</div>
          <div><b>Good with children:</b> ${escapeHtml(p.children || "-")}</div>
          <div><b>Good with pets:</b> ${escapeHtml(p.otherPets || "-")}</div>
        </div>
      </div>
      <div class="chart-card adoption-mini-card">
        <div class="sec-label">Listing Status</div>
        <div class="adoption-card-badges" style="margin-bottom:10px;">
          <span class="status ${adoptionStatusMap[p.adoptionStatus].cls}">${adoptionStatusMap[p.adoptionStatus].label}</span>
          <span class="status ${adoptionApprovalMap[p.approvalStatus].cls}">${adoptionApprovalMap[p.approvalStatus].label}</span>
        </div>
        <div class="adoption-detail-lines"><b>Description:</b><br>${escapeHtml(p.desc || "-")}</div>
      </div>
    </div>
  `;

    openModal("adoptionPetModal");
}

function deleteAdoptionPet(id) {
    if (!confirm("Delete this adoption listing?")) return;
    adoptionPets = adoptionPets.filter(p => p.id !== id);
    renderAdoptionSection();
}

function submitAdoptionPet() {
    const petName = byId("petName");
    const petType = byId("petType");
    const petBreed = byId("petBreed");
    const petAge = byId("petAge");
    const petGender = byId("petGender");
    const petColor = byId("petColor");
    const petHealth = byId("petHealth");
    const petNeuter = byId("petNeuter");
    const petVaccinated = byId("petVaccinated");
    const petVaccDate = byId("petVaccDate");
    const petProblems = byId("petProblems");
    const petTraits = byId("petTraits");
    const petEnergy = byId("petEnergy");
    const petChildren = byId("petChildren");
    const petOtherPets = byId("petOtherPets");
    const petDesc = byId("petDesc");
    const petAdoptionStatus = byId("petAdoptionStatus");
    const petApprovalStatus = byId("petApprovalStatus");
    const petImageInput = byId("petImage");

    if (!petName || !petType || !petBreed || !petAge || !petGender || !petColor ||
        !petHealth || !petNeuter || !petVaccinated || !petVaccDate || !petProblems ||
        !petTraits || !petEnergy || !petChildren || !petOtherPets || !petDesc ||
        !petAdoptionStatus || !petApprovalStatus) return;

    const name = petName.value.trim();
    const type = petType.value;
    const breed = petBreed.value.trim();
    const age = petAge.value.trim();
    const gender = petGender.value;
    const color = petColor.value.trim();
    const health = petHealth.value;
    const neuter = petNeuter.value;
    const vacc = petVaccinated.value;
    const vaccDate = petVaccDate.value;
    const problems = petProblems.value.trim();
    const traits = petTraits.value.trim();
    const energy = petEnergy.value;
    const children = petChildren.value;
    const otherPets = petOtherPets.value;
    const desc = petDesc.value.trim();
    const adoptionStatus = petAdoptionStatus.value;
    const approvalStatus = petApprovalStatus.value;
    const image = petImageInput?.dataset.imageData || "";

    if (!name || !type || !breed || !age || !gender || !health || !neuter || !vacc) {
        alert("Please fill in all required fields.");
        return;
    }

    const typeLabelMap = { dog: "Dog", cat: "Cat", bird: "Bird", other: "Other" };

    adoptionPets.unshift({
        id: Date.now(), name, type,
        typeLabel: typeLabelMap[type] || "Other",
        breed, age, gender, color, health, vacc, neuter, vaccDate,
        problems, traits, energy, children, otherPets,
        approvalStatus, adoptionStatus, desc, image
    });

    ["petName", "petBreed", "petAge", "petColor", "petVaccDate", "petProblems", "petTraits", "petDesc"].forEach(id => {
        const el = byId(id); if (el) el.value = "";
    });

    ["petType", "petGender", "petHealth", "petNeuter", "petVaccinated"].forEach(id => {
        const el = byId(id); if (el) el.selectedIndex = 0;
    });

    if (petEnergy) petEnergy.value = "Medium";
    if (petChildren) petChildren.value = "Yes";
    if (petOtherPets) petOtherPets.value = "Yes";
    if (petAdoptionStatus) petAdoptionStatus.value = "available";
    if (petApprovalStatus) petApprovalStatus.value = "pending";

    if (petImageInput) {
        petImageInput.value = "";
        delete petImageInput.dataset.imageData;
    }

    const previewWrap = qs(".preview-wrap", petImageInput?.closest(".f") || document);
    const previewImg = qs(".preview-img", petImageInput?.closest(".f") || document);
    if (previewWrap) previewWrap.style.display = "none";
    if (previewImg) previewImg.src = "";

    toast("Adoption listing submitted successfully.");
    openAdoptionManagePage();
    renderAdoptionSection();
}

function renderAdoptionSection() {
    if (!isAdoptionPage()) return;
    renderAdoptionStats();
    renderAdoptionCards();
    renderAdoptionTable();
}

function attachAdoptionEvents() {
    if (!isAdoptionPage()) return;

    const search = byId("adoptionSearch");
    const status = byId("adoptionStatusFilter");
    const approval = byId("adoptionApprovalFilter");

    if (search) search.addEventListener("input", renderAdoptionSection);
    if (status) status.addEventListener("change", renderAdoptionSection);
    if (approval) approval.addEventListener("change", renderAdoptionSection);
}

/* =========================
   Wire UI
========================= */
function wireChips() {
    if (isAppointmentsPage()) {
        qsa(".chip").forEach(chip => {
            chip.addEventListener("click", () => {
                qsa(".chip").forEach(c => c.classList.remove("active"));
                chip.classList.add("active");
                pageState.appointmentRange = chip.textContent.toLowerCase().includes("today")
                    ? "today"
                    : chip.textContent.toLowerCase().includes("week") ? "week" : "month";
                filterAppointments();
            });
        });
    }

    if (isPatientsPage()) {
        qsa(".chip").forEach(chip => {
            chip.addEventListener("click", () => {
                qsa(".chip").forEach(c => c.classList.remove("active"));
                chip.classList.add("active");
                pageState.patientVaccination = chip.textContent.toLowerCase().includes("expired") ? "expired" : "all";
                filterPatients();
            });
        });
    }
}

function wireFilters() {
    if (isAppointmentsPage()) {
        qs(".search-input input")?.addEventListener("input", filterAppointments);
        qsa(".filter-bar select").forEach(sel => sel.addEventListener("change", filterAppointments));
    }

    if (isPatientsPage()) {
        qs(".search-input input")?.addEventListener("input", filterPatients);
        qs(".filter-bar select")?.addEventListener("change", filterPatients);
    }

    if (isRecordsPage()) {
        qs(".search-input input")?.addEventListener("input", () => filterRecords());
        qs(".filter-bar select")?.addEventListener("change", () => filterRecords());
    }

    if (isRevenuePage()) {
        qsa("#tab-rev-payments select").forEach(sel => sel.addEventListener("change", filterPayments));
    }
}

function wireButtons() {
    if (isLoginPage()) {
        byId("roleManager")?.addEventListener("click", () => selectRole("manager"));
        byId("roleDoctor")?.addEventListener("click", () => selectRole("doctor"));
        byId("loginBtn")?.addEventListener("click", doLogin);
    }

    if (isAppointmentsPage()) {
        qsa("tbody tr").forEach(bindAppointmentRow);

        const apptSave = qsa("#modalAppt .modal-footer .btn, #modalAppt .modal-ft .btn").find(b => b.textContent.includes("Save"));
        if (apptSave) apptSave.onclick = addAppointment;

        const apptExport = qs(".header-actions .btn-outline");
        if (apptExport) apptExport.onclick = () => exportTable("body", "appointments.csv");
    }

    if (isPatientsPage()) {
        qsa("tbody tr").forEach(bindPatientRow);

        const petSave = qsa("#modalPet .modal-footer .btn, #modalPet .modal-ft .btn").find(b => b.textContent.includes("Save"));
        if (petSave) petSave.onclick = addPatient;
    }

    if (isRecordsPage()) {
        qsa(".record-item").forEach(bindRecordItem);

        const recSave = qsa("#modalRecord .modal-footer .btn, #modalRecord .modal-ft .btn").find(b => b.textContent.includes("Save"));
        if (recSave) recSave.onclick = addRecord;
    }

    if (isDoctorsPage()) {
        qsa(".doctor-card").forEach(bindDoctorCard);

        const docSave = qsa("#modalDoctor .modal-footer .btn, #modalDoctor .modal-ft .btn").find(b => b.textContent.includes("Add Doctor"));
        if (docSave) docSave.onclick = addDoctor;

        const docEditSave = qsa("#modalDoctorEdit .modal-footer .btn, #modalDoctorEdit .modal-ft .btn").find(b => b.textContent.includes("Save Changes"));
        if (docEditSave) {
            docEditSave.onclick = () => { closeModal("modalDoctorEdit"); toast("Doctor information updated."); };
        }

        const inviteBtns = qsa("#modalInvite .btn");
        inviteBtns.forEach(btn => {
            if (btn.textContent.includes("Copy code")) {
                btn.onclick = async () => {
                    const code = qs('#modalInvite .modal-body [style*="letter-spacing:3px"]')?.innerText || "WAFFY-DR-4X9K";
                    try { await navigator.clipboard.writeText(code); } catch (_) { }
                    toast("Invite code copied.");
                };
            }
            if (btn.textContent.includes("Send Invite")) {
                btn.onclick = () => { closeModal("modalInvite"); toast("Invite sent successfully."); };
            }
        });
    }

    if (isRevenuePage()) {
        const revenueExport = qs(".page-header .btn-outline");
        if (revenueExport) revenueExport.onclick = () => exportTable("#tab-rev-payments", "payments.csv");
    }

    if (isReportsPage()) {
        const reportExport = qs(".page-header .btn-outline");
        if (reportExport) reportExport.onclick = () => exportTable("body", "appointments-report.csv");
    }

    const notifBtn = qs(".notif-btn");
    if (notifBtn) notifBtn.onclick = () => toast("You have 3 new notifications.");

    const userBtn = qs(".user-btn");
    if (userBtn) userBtn.onclick = () => toast("Profile menu preview.");

    qsa(".sub-btn").forEach(btn => btn.addEventListener("click", () => toast(btn.textContent.trim() + " clicked.")));

    qsa(".day-chip").forEach(chip => chip.addEventListener("click", () => {
        if (!chip.hasAttribute("onclick")) {
            chip.classList.toggle("on");
            toast(`${chip.textContent.trim()} ${chip.classList.contains("on") ? "selected" : "removed"}.`);
        }
    }));
}

/* =========================
   Sidebar / Hamburger
========================= */
function syncTopProfile() {
    const a = byId("userAva")?.textContent || "M";
    const n = byId("userName")?.textContent || "Clinic Manager";
    const r = byId("roleTag")?.textContent || "Clinic Manager 👑";
    if (byId("userAvaTop")) byId("userAvaTop").textContent = a;
    if (byId("userNameTop")) byId("userNameTop").textContent = n;
    if (byId("roleTagTop")) byId("roleTagTop").textContent = r;
}

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
    qsa(".sidebar .nav-item, .wa-sb .sb-row").forEach(item => {
        item.addEventListener("click", () => {
            if (window.innerWidth <= 992) closeSidebar();
        });
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth > 992) closeSidebar();
    });
}

function normalizeHeroActions() {
    const hero = qs('.page-hero');
    if (!hero) return;
    const firstHeader = qs('.page-body > .page-content > .page-header:first-child') ||
        qs('.page-body > .page-header:first-child');
    if (!firstHeader) return;

    const titleBlock = firstHeader.querySelector(':scope > div:first-child');
    const actionButtons = Array.from(firstHeader.querySelectorAll(':scope > .btn, :scope > button, :scope > a.btn, :scope .header-actions > *'));

    if (titleBlock) titleBlock.style.display = 'none';

    if (actionButtons.length) {
        let actionWrap = hero.querySelector('.page-hero-actions');
        if (!actionWrap) {
            actionWrap = document.createElement('div');
            actionWrap.className = 'page-hero-actions';
            hero.appendChild(actionWrap);
        }
        actionButtons.forEach(btn => actionWrap.appendChild(btn));
        firstHeader.style.display = 'none';
    } else {
        firstHeader.style.display = 'none';
    }
}

/* =========================
   Availability Toggle
========================= */
function toggleAvail(btn) {
    if (!btn) return;
    const isAvailable = btn.textContent.includes("Available");
    btn.textContent = isAvailable ? "🔴 Unavailable" : "🟢 Available for booking";
    btn.classList.toggle("btn-primary", !isAvailable);
    btn.classList.toggle("btn-outline", isAvailable);
    toast(isAvailable ? "Booking disabled temporarily." : "Booking enabled.");
}

/* =========================
   Notification Toggle
========================= */
function toggleNotif(btn) {
    if (!btn) return;
    const isEnabled = btn.textContent.trim() === "Enabled";
    btn.textContent = isEnabled ? "Off" : "Enabled";
    btn.classList.toggle("btn-primary", !isEnabled);
    btn.classList.toggle("btn-outline", isEnabled);
}

/* =========================
   Startup
========================= */
document.addEventListener("DOMContentLoaded", () => {
    const savedRole = localStorage.getItem("waffyClinicRole");
    if (savedRole) currentRole = savedRole;

    attachRealImageUploads();
    initClinicMap();
    bindSidebarAutoClose();
    syncTopProfile();
    normalizeHeroActions();

    if (isLoginPage()) {
        wireButtons();
        selectRole(currentRole);
        return;
    }

    const loggedIn = localStorage.getItem("waffyClinicLoggedIn");
    if (loggedIn !== "true") {
        // ── MVC route ──
        window.location.href = "/ClinicAdmin/Login";
        return;
    }

    attachAdoptionEvents();
    setupData();
    wireChips();
    wireFilters();
    wireButtons();
    applyRole();

    // Guard: redirect doctor away from manager-only pages
    if (currentRole === "doctor") {
        if ((isDoctorsPage() || isRevenuePage() || isSettingsPage()) && !isDoctorSettingsPage()) {
            // ── MVC route ──
            window.location.href = "/ClinicAdmin/My_Settings";
            return;
        }
    }

    if (isAppointmentsPage()) filterAppointments();
    if (isPatientsPage()) filterPatients();

    const prefill = sessionStorage.getItem("waffy_record_prefill");
    if (isRecordsPage()) {
        filterRecords(prefill || "");
        sessionStorage.removeItem("waffy_record_prefill");
    }

    if (isRevenuePage()) filterPayments();

    if (isAdoptionPage()) {
        setAdoptionMode("clinic");
        renderAdoptionSection();
    }

    initCharts();
});

window.addEventListener("resize", resizeAllCharts);
window.addEventListener("orientationchange", resizeAllCharts);