function safeStr(val) {
    return (val ?? "").toString();
}

const filterButtons = document.querySelectorAll(".filter-item");
const modal = document.getElementById("detailsModal");
const modalContent = document.getElementById("modalContent");
const toast = document.getElementById("toast");
const searchInput = document.getElementById("petSearch");
const petsGrid = document.getElementById("petsGrid");

let currentFilter = "all";
let currentSearch = "";


// ══════════════════════════════════
// FILTER
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

filterButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        filterButtons.forEach(b => b.classList.remove("active"));
        btn.classList.add("active");
        currentFilter = btn.dataset.filter;
        applyFilters();
    });
});

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
    const btn = petsGrid?.querySelector(`[data-details="${petId}"]`);
    if (!btn || !modalContent) return;

    const name = safeStr(btn.dataset.name);
    const breed = safeStr(btn.dataset.breed);
    const age = safeStr(btn.dataset.age);
    const gender = safeStr(btn.dataset.gender);
    const city = safeStr(btn.dataset.city);
    const area = safeStr(btn.dataset.area);
    const color = safeStr(btn.dataset.color);
    const temperament = safeStr(btn.dataset.temperament);
    const health = safeStr(btn.dataset.health);
    const description = safeStr(btn.dataset.description);
    const phone = safeStr(btn.dataset.phone);
    const contact = safeStr(btn.dataset.contact);
    const vacc = safeStr(btn.dataset.vacc);
    const image = safeStr(btn.dataset.image) || "../Images/adapt/golden.png";
    const code = safeStr(btn.dataset.code);

    const type = safeStr(btn.closest("article")?.dataset.type);
    const typeEmoji = { dog: "🐕", cat: "🐱", bird: "🐦", rabbit: "🐰", hamster: "🐹", snake: "🐍" }[type] ?? "🐾";
    const vaccOk = vacc.toLowerCase().includes("vaccinated") && !vacc.includes("Not");

    modalContent.innerHTML = `
        <div class="modal-layout">
            <div class="modal-image-box">
                <img src="${image}" alt="${name}" />
            </div>
            <div class="modal-info">
                <h3>${typeEmoji} ${name}</h3>
                <div style="display:flex;gap:.5rem;flex-wrap:wrap;margin-bottom:1rem">
                    <span style="background:rgba(151,173,77,0.16);color:#59652a;
                                 padding:.25rem .9rem;border-radius:999px;
                                 font-size:.8rem;font-weight:800">
                        Available for Adoption 🐾
                    </span>
                </div>
                <div class="modal-grid">
                    ${breed ? `
                    <div class="modal-box" style="padding:.7rem 1rem">
                        <div style="font-size:.72rem;color:var(--muted);font-weight:800;text-transform:uppercase;margin-bottom:.25rem">🐾 Breed</div>
                        <strong style="color:var(--text)">${breed}</strong>
                    </div>` : ""}
                    <div class="modal-box" style="padding:.7rem 1rem">
                        <div style="font-size:.72rem;color:var(--muted);font-weight:800;text-transform:uppercase;margin-bottom:.25rem">🎂 Age</div>
                        <strong style="color:var(--text)">${age || "Unknown"}</strong>
                    </div>
                    <div class="modal-box" style="padding:.7rem 1rem">
                        <div style="font-size:.72rem;color:var(--muted);font-weight:800;text-transform:uppercase;margin-bottom:.25rem">⚧ Gender</div>
                        <strong style="color:var(--text)">${gender || "Unknown"}</strong>
                    </div>
                    ${color ? `
                    <div class="modal-box" style="padding:.7rem 1rem">
                        <div style="font-size:.72rem;color:var(--muted);font-weight:800;text-transform:uppercase;margin-bottom:.25rem">🎨 Color</div>
                        <strong style="color:var(--text)">${color}</strong>
                    </div>` : ""}
                    <div class="modal-box" style="padding:.7rem 1rem">
                        <div style="font-size:.72rem;color:var(--muted);font-weight:800;text-transform:uppercase;margin-bottom:.25rem">📍 Location</div>
                        <strong style="color:var(--text)">${city}${area ? " — " + area : ""}</strong>
                    </div>
                    <div class="modal-box" style="padding:.7rem 1rem">
                        <div style="font-size:.72rem;color:var(--muted);font-weight:800;text-transform:uppercase;margin-bottom:.25rem">⚡ Temperament</div>
                        <strong style="color:var(--text)">${temperament || "Friendly"}</strong>
                    </div>
                </div>
                ${description ? `
                <div class="modal-box" style="margin-top:.8rem">
                    <h4>📝 About ${name}</h4>
                    <p>${description}</p>
                </div>` : ""}
                <div class="modal-box" style="margin-top:.8rem">
                    <h4>🏥 Health & Care</h4>
                    <div style="display:flex;flex-wrap:wrap;gap:.4rem;margin-top:.4rem">
                        <span style="background:${vaccOk ? "#e8f5e9" : "#fce4ec"};
                                     color:${vaccOk ? "#2e7d32" : "#c62828"};
                                     padding:.8rem .8rem;border-radius:10px;
                                     font-size:.78rem;font-weight:800">
                            ${vacc || "Unknown"}
                        </span>
                        <span style="background:#e3f2fd;color:#1565c0;
                                     padding:.8rem .8rem;border-radius:10px;
                                     font-size:.78rem;font-weight:800">
                            ${health || "Healthy"}
                        </span>
                    </div>
                </div>
                <div class="modal-box" style="margin-top:.8rem;
                     background:rgba(255,248,225,0.8);
                     border-color:rgba(255,224,130,0.5)">
                    <h4 style="color:#e65100">📞 Contact Information</h4>
                    ${contact ? `<p><strong>Name:</strong> ${contact}</p>` : ""}
                    ${phone ? `<p><strong>Phone:</strong> ${phone}</p>` : ""}
                    <p style="font-size:.78rem;color:var(--muted);margin-top:.4rem">
                        Contact the owner directly to arrange adoption.
                    </p>
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
// LIKE — click handler
// ══════════════════════════════════
document.addEventListener("click", async e => {
    const likeBtn = e.target.closest("[data-like]");
    if (!likeBtn) return;

    const isLoggedIn = document.body.dataset.loggedIn === "true";
    if (!isLoggedIn) {
        showToast("❤️ Please login to like this pet!");
        return;
    }

    const postId = likeBtn.dataset.like;
    const counter = document.getElementById(`likes-${postId}`);

    try {
        const res = await fetch(`/Pet/ToggleLike?postId=${postId}`, {
            method: "POST",
            headers: {
                "RequestVerificationToken": document.querySelector('[name="__RequestVerificationToken"]')?.value ?? ""
            }
        });

        const data = await res.json();

        // ✅ الـ server يقرر liked أو لا — مش localStorage
        if (data.liked) {
            likeBtn.classList.add("active");
        } else {
            likeBtn.classList.remove("active");
        }

        if (counter) counter.textContent = data.count;

    } catch (err) {
        console.error("Like error:", err);
    }
});

// ══════════════════════════════════
// DETAILS + CLOSE — click handler
// ══════════════════════════════════
document.addEventListener("click", e => {
    const detailsBtn = e.target.closest("[data-details]");
    const closeBtn = e.target.closest("[data-close-modal]");

    if (detailsBtn) {
        openDetails(detailsBtn.dataset.details);
        return;
    }
    if (closeBtn) closeDetails();


});


document.addEventListener("keydown", e => {
    if (e.key === "Escape") {
        closeDetails();
        closeReserveModal(); 
    }
});

// ══════════════════════════════════
// RESERVE MODAL
// ══════════════════════════════════
function openReserveModal(petId, petName) {
    document.getElementById("reservePostId").value = petId;
    document.getElementById("reservePetName").textContent = petName;
    document.getElementById("reserveModal").classList.remove("hidden");
}

function closeReserveModal() {
    document.getElementById("reserveModal").classList.add("hidden");
}

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
// INIT — مرة واحدة فقط
// ══════════════════════════════════
document.addEventListener("DOMContentLoaded", () => {
    initFadeUpAnimation();
    initDesktopServicesMenu();
    initMobileMenu();


    // ✅ أغلق reserveModal عند الضغط على الـ overlay
    document.getElementById("reserveModal")
        ?.querySelector(".modal-overlay")
        ?.addEventListener("click", closeReserveModal);
});