function safeStr(val) {
    return (val ?? "").toString();
}

// ✅ البيانات للـ Modal فقط — تُجمع من الـ DOM
const filterButtons = document.querySelectorAll(".filter-item");
const modal = document.getElementById("detailsModal");
const modalContent = document.getElementById("modalContent");
const toast = document.getElementById("toast");
const searchInput = document.getElementById("petSearch");
const petsGrid = document.getElementById("petsGrid");

let currentFilter = "all";
let currentSearch = "";

// ══════════════════════════════════
// FILTER — يخفي ويظهر الكروت
// ══════════════════════════════════
function applyFilters() {
    const articles = petsGrid?.querySelectorAll("article.pet-item") ?? [];
    const query = currentSearch.trim().toLowerCase();

    articles.forEach(card => {
        const type = safeStr(card.dataset.type).toLowerCase();
        const text = safeStr(card.textContent).toLowerCase();

        const matchFilter = currentFilter === "all" || type === currentFilter;
        const matchSearch = !query || text.includes(query);

        card.style.display = (matchFilter && matchSearch) ? "" : "none";
    });

    // ✅ لو كل الكروت مخفية — أظهر رسالة
    const visible = [...articles].filter(c => c.style.display !== "none");
    const empty = petsGrid?.querySelector(".empty-state");

    if (empty) {
        empty.style.display = visible.length === 0 ? "" : "none";
    } else if (visible.length === 0) {
        petsGrid?.insertAdjacentHTML("beforeend", `
            <div class="empty-state" id="dynamicEmpty">
                <h3>No pets found</h3>
                <p>Try another filter or search keyword.</p>
            </div>
        `);
    } else {
        document.getElementById("dynamicEmpty")?.remove();
    }
}

// ══════════════════════════════════
// FILTER BUTTONS
// ══════════════════════════════════
filterButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        filterButtons.forEach(b => b.classList.remove("active"));
        btn.classList.add("active");
        currentFilter = btn.dataset.filter;
        applyFilters();
    });
});

// ══════════════════════════════════
// SEARCH
// ══════════════════════════════════
if (searchInput) {
    searchInput.addEventListener("input", e => {
        currentSearch = e.target.value;
        applyFilters();
    });
}

// ══════════════════════════════════
// MODAL — View Details
// ══════════════════════════════════
function openDetails(petId) {
    // ✅ نقرأ البيانات من الـ DOM مباشرة
    const card = petsGrid?.querySelector(`[data-like="${petId}"]`)?.closest("article");
    if (!card || !modalContent) return;

    const name = safeStr(card.querySelector("h3")?.textContent);
    const paragraphs = card.querySelectorAll(".pet-info p");
    const img = card.querySelector("img")?.src ?? "";

    modalContent.innerHTML = `
        <div class="modal-layout">
            <div class="modal-image-box">
                <img src="${img}" alt="${name}">
            </div>
            <div class="modal-info">
                <h3>${name}</h3>
                ${[...paragraphs].map(p => `<p>${p.innerHTML}</p>`).join("")}
                <div class="modal-box">
                    <h4>Adoption Notes</h4>
                    <ul>
                        <li>Friendly with gentle handling</li>
                        <li>Needs a loving home</li>
                        <li>Vet-checked before adoption</li>
                        <li>Ready for reservation</li>
                    </ul>
                </div>
            </div>
        </div>
    `;

    modal?.classList.remove("hidden");
}

function closeDetails() {
    modal?.classList.add("hidden");
}

// ══════════════════════════════════
// TOAST
// ══════════════════════════════════
function showToast(message) {
    if (!toast) return;
    toast.textContent = message;
    toast.classList.add("show");
    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => toast.classList.remove("show"), 3200);
}

// ══════════════════════════════════
// CLICK EVENTS
// ══════════════════════════════════
document.addEventListener("click", e => {
    const likeBtn = e.target.closest("[data-like]");
    const reserveBtn = e.target.closest("[data-reserve]");
    const detailsBtn = e.target.closest("[data-details]");
    const closeBtn = e.target.closest("[data-close-modal]");

    if (likeBtn) {
        const counter = document.getElementById(`likes-${likeBtn.dataset.like}`);
        if (counter) counter.textContent = Number(counter.textContent) + 1;
        likeBtn.classList.add("active");
        return;
    }

    if (reserveBtn) {
        const card = reserveBtn.closest("article");
        const name = safeStr(card?.querySelector("h3")?.textContent);
        showToast(`Thank you for reserving ${name}! Your reservation has been received.`);
        return;
    }

    if (detailsBtn) {
        openDetails(detailsBtn.dataset.details);
        return;
    }

    if (closeBtn) closeDetails();
});

document.addEventListener("keydown", e => {
    if (e.key === "Escape") closeDetails();
});

// ══════════════════════════════════
// NAVBAR
// ══════════════════════════════════
function initFadeUpAnimation() {
    const els = document.querySelectorAll(".fade-up");
    if ("IntersectionObserver" in window) {
        const obs = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("show");
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15 });
        els.forEach(el => obs.observe(el));
    } else {
        els.forEach(el => el.classList.add("show"));
    }
}

function initDesktopServicesMenu() {
    const dropdown = document.getElementById("servicesDropdown");
    const toggle = document.getElementById("servicesToggle");
    if (!dropdown || !toggle) return;

    toggle.addEventListener("click", e => {
        e.stopPropagation();
        dropdown.classList.toggle("open");
    });
    document.addEventListener("click", e => {
        if (!dropdown.contains(e.target)) dropdown.classList.remove("open");
    });
}

function initMobileMenu() {
    const menuBtn = document.getElementById("menuBtn");
    const navLinks = document.getElementById("navLinks");
    const mobileServicesBlock = document.getElementById("mobileServicesBlock");
    const mobileServicesToggle = document.getElementById("mobileServicesToggle");

    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", e => {
            e.stopPropagation();
            navLinks.classList.toggle("open");
        });
        document.addEventListener("click", e => {
            if (!navLinks.contains(e.target) && e.target !== menuBtn)
                navLinks.classList.remove("open");
        });
    }

    if (mobileServicesBlock && mobileServicesToggle) {
        mobileServicesToggle.addEventListener("click", () =>
            mobileServicesBlock.classList.toggle("open")
        );
    }
}

// ══════════════════════════════════
// INIT
// ══════════════════════════════════
document.addEventListener("DOMContentLoaded", () => {
    initFadeUpAnimation();
    initDesktopServicesMenu();
    initMobileMenu();
});