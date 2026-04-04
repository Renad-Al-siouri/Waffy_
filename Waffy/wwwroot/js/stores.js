const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");

const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");

const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

const storeSearch = document.getElementById("storeSearch");
const storeCards = document.querySelectorAll(".store-card");
const emptyState = document.getElementById("emptyState");

function initServicesDropdown() {
    if (!servicesDropdown || !servicesToggle) return;

    servicesToggle.addEventListener("click", function (e) {
        e.preventDefault();
        e.stopPropagation();
        servicesDropdown.classList.toggle("open");
    });

    document.addEventListener("click", function (e) {
        if (!servicesDropdown.contains(e.target)) {
            servicesDropdown.classList.remove("open");
        }
    });
}

function initMobileMenu() {
    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            navLinks.classList.toggle("open");
        });

        document.addEventListener("click", function (e) {
            const clickedInsideMenu = navLinks.contains(e.target);
            const clickedMenuBtn = menuBtn.contains(e.target);

            if (!clickedInsideMenu && !clickedMenuBtn) {
                navLinks.classList.remove("open");
            }
        });
    }

    if (mobileServicesBlock && mobileServicesToggle) {
        mobileServicesToggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            mobileServicesBlock.classList.toggle("open");
        });
    }
}

function filterStores() {
    if (!storeCards.length) return;

    const searchValue = (storeSearch?.value || "").trim().toLowerCase();
    let visibleCount = 0;

    storeCards.forEach((card) => {
        const cardSearch = (card.dataset.search || "").toLowerCase();
        const matchesSearch = !searchValue || cardSearch.includes(searchValue);

        if (matchesSearch) {
            card.style.display = "";
            visibleCount++;
        } else {
            card.style.display = "none";
        }
    });

    if (emptyState) {
        emptyState.classList.toggle("show", visibleCount === 0);
    }
}

function initStoreSearch() {
    if (!storeSearch) return;
    storeSearch.addEventListener("input", filterStores);
    filterStores();
}

function initFadeUpAnimation() {
    const fadeElements = document.querySelectorAll(".fade-up");

    if ("IntersectionObserver" in window) {
        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add("show");
                        observer.unobserve(entry.target);
                    }
                });
            },
            { threshold: 0.15 }
        );

        fadeElements.forEach((el) => observer.observe(el));
    } else {
        fadeElements.forEach((el) => el.classList.add("show"));
    }
}

document.addEventListener("DOMContentLoaded", function () {
    initServicesDropdown();
    initMobileMenu();
    initStoreSearch();
    initFadeUpAnimation();
});