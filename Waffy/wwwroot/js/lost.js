// ══════════════════════════════════
// ELEMENTS
// ══════════════════════════════════
const petsGrid = document.getElementById("petsGrid");
const lostSearch = document.getElementById("lostSearch");
const filterButtons = document.querySelectorAll(".filter-item");
const toast = document.getElementById("toast");
const flowModal = document.getElementById("flowModal");
const flowSteps = document.querySelectorAll(".flow-step");
const detailsModal = document.getElementById("detailsModal");
const detailsContent = document.getElementById("detailsContent");

const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

let currentFilter = "all";
let currentSearch = "";
let currentPetId = null;
let currentSecretQ = "";

// ══════════════════════════════════
// FILTER
// ══════════════════════════════════
function applyFilters() {
    const articles = petsGrid?.querySelectorAll("article.pet-item") ?? [];
    const query = currentSearch.trim().toLowerCase();
    articles.forEach(card => {
        const type = (card.dataset.type || "").toLowerCase();
        const text = card.textContent.toLowerCase();
        const matchFilter = currentFilter === "all" || type === currentFilter;
        const matchSearch = !query || text.includes(query);
        card.style.display = (matchFilter && matchSearch) ? "" : "none";
    });
    const visible = [...articles].filter(c => c.style.display !== "none");
    const empty = document.getElementById("dynamicEmpty");
    if (empty) empty.style.display = visible.length === 0 ? "" : "none";
}

filterButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        filterButtons.forEach(b => b.classList.remove("active"));
        btn.classList.add("active");
        currentFilter = btn.dataset.filter;
        applyFilters();
    });
});

if (lostSearch) {
    lostSearch.addEventListener("input", e => {
        currentSearch = e.target.value;
        applyFilters();
    });
}

// ══════════════════════════════════
// DETAILS MODAL — با carousel للصور
// ══════════════════════════════════
function openDetails(petId) {
    const card = petsGrid?.querySelector(`[data-id="${petId}"]`);
    if (!card || !detailsContent) return;

    const name = card.dataset.name;
    const type = card.dataset.typeDisplay;
    const breed = card.dataset.breed;
    const color = card.dataset.color;
    const city = card.dataset.city;
    const area = card.dataset.area;
    const desc = card.dataset.description;
    const marks = card.dataset.marks;
    const phone = card.dataset.phone;
    const reward = card.dataset.reward;
    const hasRew = card.dataset.hasReward === "true";
    const date = card.dataset.date;
    const expiry = card.dataset.expiry;

    // ✅ قراءة كل الصور من data-images (pipe-separated)
    const imagesRaw = card.dataset.images || card.dataset.image || "";
    const images = imagesRaw
        .split("|")
        .map(s => s.trim())
        .filter(s => s.length > 0);
    const mainImage = images[0] || "/Images/adapt/dog.png";

    const typeEmoji = { dog: "🐕", cat: "🐱", bird: "🐦", rabbit: "🐰", hamster: "🐹", snake: "🐍" }
    [card.dataset.type] ?? "🐾";

    // ── بناء الـ carousel HTML — show/hide ──
    const carouselHtml = images.length > 1
        ? `<div id="mdCarousel" style="position:relative;border-radius:14px;overflow:hidden;background:#f5f5f5">
                ${images.map((src, i) => `
                    <img src="${src}" alt="${name}" id="mdImg${i}"
                         style="width:100%;height:220px;object-fit:cover;border-radius:14px;display:${i === 0 ? 'block' : 'none'}">
                `).join("")}
                <button onclick="mdCarMove(-1)"
                        style="position:absolute;top:50%;left:8px;transform:translateY(-50%);
                               width:28px;height:28px;border-radius:50%;background:rgba(255,255,255,.85);
                               border:none;font-size:15px;font-weight:700;cursor:pointer;
                               display:flex;align-items:center;justify-content:center;color:#3d7a10;z-index:2">‹</button>
                <button onclick="mdCarMove(1)"
                        style="position:absolute;top:50%;right:8px;transform:translateY(-50%);
                               width:28px;height:28px;border-radius:50%;background:rgba(255,255,255,.85);
                               border:none;font-size:15px;font-weight:700;cursor:pointer;
                               display:flex;align-items:center;justify-content:center;color:#3d7a10;z-index:2">›</button>
                <div style="position:absolute;bottom:7px;left:50%;transform:translateX(-50%);
                            display:flex;gap:5px;z-index:2">
                    ${images.map((_, i) => `
                        <button onclick="mdCarGo(${i})" id="mdDot${i}"
                                style="width:7px;height:7px;border-radius:50%;border:none;
                                       padding:0;cursor:pointer;
                                       background:${i === 0 ? "#fff" : "rgba(255,255,255,.5)"};
                                       transition:background .2s"></button>
                    `).join("")}
                </div>
                <span id="mdCounter"
                      style="position:absolute;top:7px;right:10px;background:rgba(0,0,0,.45);
                             color:#fff;font-size:.68rem;font-weight:700;padding:2px 7px;
                             border-radius:20px;z-index:2">1 / ${images.length}</span>
           </div>`
        : `<img src="${mainImage}" alt="${name}"
                style="width:100%;border-radius:14px;object-fit:cover;max-height:220px">`;

    detailsContent.innerHTML = `
        <div style="display:flex;gap:1.5rem;flex-wrap:wrap">
            <div style="flex:0 0 220px;max-width:220px">
                ${carouselHtml}
                ${hasRew ? `<div style="background:#fff8e1;border:1px solid #ffe082;
                    border-radius:10px;padding:.6rem;margin-top:.8rem;
                    text-align:center;font-weight:800;color:#e65100">
                    🎁 Reward: ${reward} JOD
                </div>` : ""}
            </div>
            <div style="flex:1;min-width:200px">
                <h3 style="font-family:'Fredoka',sans-serif;color:#59652a;font-size:1.4rem;margin-bottom:.5rem">
                    ${typeEmoji} ${name}
                </h3>
                <div style="display:flex;flex-wrap:wrap;gap:.4rem;margin-bottom:1rem">
                    <span style="background:#fce4ec;color:#c62828;padding:.2rem .8rem;
                          border-radius:999px;font-size:.78rem;font-weight:800">🔴 Missing</span>
                    ${date ? `<span style="background:#f3e5f5;color:#6a1b9a;padding:.2rem .8rem;
                          border-radius:999px;font-size:.78rem;font-weight:800">📅 Lost: ${date}</span>` : ""}
                    ${expiry ? `<span style="background:#e8f5e9;color:#2e7d32;padding:.2rem .8rem;
                          border-radius:999px;font-size:.78rem;font-weight:800">⏳ Expires: ${expiry}</span>` : ""}
                </div>
                <div style="display:grid;grid-template-columns:1fr 1fr;gap:.6rem;margin-bottom:.8rem">
                    <div style="background:#f8fdf2;border:1px solid #d4e8bc;border-radius:10px;padding:.6rem .8rem">
                        <div style="font-size:.7rem;color:#7b6c58;font-weight:800;text-transform:uppercase">🐾 Type</div>
                        <strong>${type || "Unknown"}</strong>
                    </div>
                    ${breed ? `<div style="background:#f8fdf2;border:1px solid #d4e8bc;border-radius:10px;padding:.6rem .8rem">
                        <div style="font-size:.7rem;color:#7b6c58;font-weight:800;text-transform:uppercase">🐕 Breed</div>
                        <strong>${breed}</strong>
                    </div>` : ""}
                    <div style="background:#f8fdf2;border:1px solid #d4e8bc;border-radius:10px;padding:.6rem .8rem">
                        <div style="font-size:.7rem;color:#7b6c58;font-weight:800;text-transform:uppercase">🎨 Color</div>
                        <strong>${color || "Unknown"}</strong>
                    </div>
                    <div style="background:#f8fdf2;border:1px solid #d4e8bc;border-radius:10px;padding:.6rem .8rem">
                        <div style="font-size:.7rem;color:#7b6c58;font-weight:800;text-transform:uppercase">📍 Location</div>
                        <strong>${city}${area ? " — " + area : ""}</strong>
                    </div>
                </div>
                ${desc ? `<div style="background:#f8fdf2;border:1px solid #d4e8bc;border-radius:10px;padding:.8rem;margin-bottom:.6rem">
                    <strong>📝 Description</strong>
                    <p style="margin:.3rem 0 0;color:#444;font-size:.9rem">${desc}</p>
                </div>` : ""}
                ${marks ? `<div style="background:#fff8e1;border:1px solid #ffe082;border-radius:10px;padding:.8rem;margin-bottom:.6rem">
                    <strong>⭐ Special Marks</strong>
                    <p style="margin:.3rem 0 0;color:#444;font-size:.9rem">${marks}</p>
                </div>` : ""}
                ${phone ? `<div style="background:#e8f5e9;border:1px solid #b2dfb2;border-radius:10px;padding:.8rem;margin-bottom:.8rem">
                    <strong style="color:#2d7a2d">📞 Contact Owner</strong>
                    <p style="margin:.3rem 0 0;font-size:1rem;font-weight:800;color:#1b5e20">${phone}</p>
                </div>` : ""}
                <button class="step-btn" onclick="closeDetails(); openFlow('${petId}');"
                        style="width:100%;margin-top:.5rem">🔍 I Found This Pet</button>
            </div>
        </div>
    `;

    // ✅ متغير الـ carousel الحالي للـ modal
    window._mdIdx = 0;
    window._mdCount = images.length;

    detailsModal?.classList.remove("hidden");
}

// ✅ تحريك الـ carousel داخل الـ modal — show/hide
window.mdCarMove = function (dir) {
    window._mdIdx = ((window._mdIdx + dir) + window._mdCount) % window._mdCount;
    mdCarGo(window._mdIdx);
};
window.mdCarGo = function (idx) {
    window._mdIdx = idx;
    // ✅ show/hide بدل translateX
    for (let i = 0; i < window._mdCount; i++) {
        const img = document.getElementById(`mdImg${i}`);
        if (img) img.style.display = i === idx ? 'block' : 'none';
    }
    const counter = document.getElementById("mdCounter");
    if (counter) counter.textContent = `${idx + 1} / ${window._mdCount}`;
    for (let i = 0; i < window._mdCount; i++) {
        const dot = document.getElementById(`mdDot${i}`);
        if (dot) dot.style.background = i === idx ? "#fff" : "rgba(255,255,255,.5)";
    }
};

function closeDetails() {
    detailsModal?.classList.add("hidden");
}

// ══════════════════════════════════
// I FOUND IT FLOW
// ══════════════════════════════════
function showStep(n) {
    flowSteps.forEach(s => s.classList.remove("active"));
    document.querySelector(`.flow-step[data-step="${n}"]`)?.classList.add("active");
}

function openFlow(petId) {
    const isLoggedIn = document.body.dataset.loggedIn === "true";
    if (!isLoggedIn) {
        showToast("⚠️ Please log in to report a found pet.");
        setTimeout(() => { window.location.href = "/Account/Login"; }, 1500);
        return;
    }
    const card = petsGrid?.querySelector(`[data-id="${petId}"]`);
    if (!card) return;
    currentPetId = petId;
    currentSecretQ = card.dataset.secretQuestion || "";
    const qDisplay = document.getElementById("secretQuestionDisplay");
    if (qDisplay) qDisplay.textContent = currentSecretQ || "No secret question set.";
    ["finderName", "finderPhone", "secretAnswer"].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = "";
    });
    flowModal?.classList.remove("hidden");
    showStep(1);
}

function closeFlow() { flowModal?.classList.add("hidden"); }

async function goToVerificationStep() {
    const name = document.getElementById("finderName")?.value.trim() || "";
    const phone = document.getElementById("finderPhone")?.value.trim() || "";
    const answer = document.getElementById("secretAnswer")?.value.trim() || "";
    if (!name || !phone || !answer) { showToast("⚠️ Please fill in all fields."); return; }

    const btn = document.querySelector("[onclick='goToVerificationStep()']");
    if (btn) { btn.disabled = true; btn.textContent = "Submitting..."; }

    try {
        const res = await fetch("/FoundPetClaim/Submit", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({ reportId: currentPetId, finderName: name, finderPhone: phone, secretAnswer: answer })
        });
        const data = await res.json();
        if (!data.success) {
            showToast("❌ " + data.message);
            if (btn) { btn.disabled = false; btn.textContent = "Submit for Verification →"; }
            return;
        }
        const resultEl = document.getElementById("verificationResult");
        if (resultEl) {
            resultEl.textContent = data.message;
            resultEl.style.background = data.verified ? "#e8f5e9" : "#fff8e1";
            resultEl.style.border = data.verified ? "1px solid #b2dfb2" : "1px solid #ffe082";
            resultEl.style.color = data.verified ? "#2d7a2d" : "#92400e";
            resultEl.style.borderRadius = "10px";
            resultEl.style.padding = ".8rem 1rem";
            resultEl.style.fontWeight = "700";
        }
        const phoneEl = document.getElementById("displayPhone");
        if (phoneEl) phoneEl.textContent = phone;
        showStep(2);
    } catch (err) {
        showToast("❌ Something went wrong. Please try again.");
        if (btn) { btn.disabled = false; btn.textContent = "Submit for Verification →"; }
    }
}

// ══════════════════════════════════
// CLICK HANDLERS
// ══════════════════════════════════
document.addEventListener("click", e => {
    const openFlowBtn = e.target.closest("[data-open-flow]");
    const openDetailsBtn = e.target.closest("[data-open-details]");
    const closeFlowBtn = e.target.closest("[data-close-flow]");

    if (openFlowBtn) { openFlow(openFlowBtn.dataset.openFlow); return; }
    if (openDetailsBtn) { openDetails(openDetailsBtn.dataset.openDetails); return; }
    if (closeFlowBtn) { closeFlow(); return; }
    if (e.target === detailsModal || e.target.id === "detailsOverlay") { closeDetails(); return; }
});

document.getElementById("closeDetailsBtn")?.addEventListener("click", closeDetails);
document.addEventListener("keydown", e => { if (e.key === "Escape") { closeFlow(); closeDetails(); } });

// ══════════════════════════════════
// TOAST
// ══════════════════════════════════
function showToast(message) {
    if (!toast) return;
    toast.textContent = message;
    toast.classList.add("show");
    clearTimeout(showToast.timer);
    showToast.timer = setTimeout(() => toast.classList.remove("show"), 3000);
}

// ══════════════════════════════════
// NAVBAR
// ══════════════════════════════════
function initServicesDropdown() {
    if (!servicesToggle || !servicesDropdown) return;
    servicesToggle.addEventListener("click", e => { e.stopPropagation(); servicesDropdown.classList.toggle("open"); });
    document.addEventListener("click", e => { if (!servicesDropdown.contains(e.target)) servicesDropdown.classList.remove("open"); });
}
function initMobileMenu() {
    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", e => { e.stopPropagation(); navLinks.classList.toggle("open"); });
        document.addEventListener("click", e => { if (!navLinks.contains(e.target) && !menuBtn.contains(e.target)) navLinks.classList.remove("open"); });
    }
    if (mobileServicesBlock && mobileServicesToggle)
        mobileServicesToggle.addEventListener("click", () => mobileServicesBlock.classList.toggle("open"));
}
function initFadeUpAnimation() {
    const els = document.querySelectorAll(".fade-up");
    if ("IntersectionObserver" in window) {
        const obs = new IntersectionObserver(entries => {
            entries.forEach(entry => { if (entry.isIntersecting) { entry.target.classList.add("show"); obs.unobserve(entry.target); } });
        }, { threshold: 0.15 });
        els.forEach(el => obs.observe(el));
    } else { els.forEach(el => el.classList.add("show")); }
}
document.addEventListener("DOMContentLoaded", () => { initFadeUpAnimation(); initServicesDropdown(); initMobileMenu(); });