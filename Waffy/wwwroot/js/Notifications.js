const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");

const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");

const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

const searchInput = document.getElementById("notificationSearch");
const categoryButtons = document.querySelectorAll(".category-btn");
const notificationCards = document.querySelectorAll(".notification-card");
const emptyState = document.getElementById("emptyState");
const markAllBtn = document.getElementById("markAllBtn");
const navNotifCount = document.getElementById("navNotifCount");

let currentFilter = "all";

function initServicesDropdown() {
    if (!servicesDropdown || !servicesToggle) return;

    servicesToggle.addEventListener("click", (e) => {
        e.preventDefault();
        e.stopPropagation();
        servicesDropdown.classList.toggle("open");
    });

    document.addEventListener("click", (e) => {
        if (!servicesDropdown.contains(e.target)) {
            servicesDropdown.classList.remove("open");
        }
    });
}

function initMobileMenu() {
    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", (e) => {
            e.preventDefault();
            e.stopPropagation();
            navLinks.classList.toggle("open");
        });

        document.addEventListener("click", (e) => {
            const clickedInsideMenu = navLinks.contains(e.target);
            const clickedMenuBtn = menuBtn.contains(e.target);

            if (!clickedInsideMenu && !clickedMenuBtn) {
                navLinks.classList.remove("open");
            }
        });
    }

    if (mobileServicesBlock && mobileServicesToggle) {
        mobileServicesToggle.addEventListener("click", (e) => {
            e.preventDefault();
            e.stopPropagation();
            mobileServicesBlock.classList.toggle("open");
        });
    }
}

function updateUnreadCount() {
    const unreadCount = document.querySelectorAll(".notification-card.unread").length;
    if (navNotifCount) {
        navNotifCount.textContent = unreadCount;
        navNotifCount.style.display = unreadCount ? "flex" : "none";
    }
}

function filterNotifications() {
    const searchValue = (searchInput?.value || "").trim().toLowerCase();
    let visibleCount = 0;

    notificationCards.forEach((card) => {
        const type = card.dataset.type || "";
        const searchableText = (card.dataset.search || "").toLowerCase();

        const matchesFilter = currentFilter === "all" ? true : type === currentFilter;
        const matchesSearch = !searchValue || searchableText.includes(searchValue);

        if (matchesFilter && matchesSearch) {
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

function initFilterButtons() {
    categoryButtons.forEach((button) => {
        button.addEventListener("click", () => {
            categoryButtons.forEach((btn) => btn.classList.remove("active"));
            button.classList.add("active");
            currentFilter = button.dataset.filter;
            filterNotifications();
        });
    });
}

function initSearch() {
    if (!searchInput) return;
    searchInput.addEventListener("input", filterNotifications);
}

function initNotificationActions() {
    document.addEventListener("click", (e) => {
        const secondaryBtn = e.target.closest(".notif-btn.secondary");
        if (!secondaryBtn) return;

        const card = secondaryBtn.closest(".notification-card");
        if (!card) return;

        card.classList.remove("unread");
        const badge = card.querySelector(".notification-badge");
        if (badge && (badge.textContent === "New" || badge.textContent === "Reminder")) {
            badge.remove();
        }

        updateUnreadCount();
    });
}

function initMarkAllAsRead() {
    if (!markAllBtn) return;

    markAllBtn.addEventListener("click", () => {
        notificationCards.forEach((card) => {
            card.classList.remove("unread");
            const badge = card.querySelector(".notification-badge");
            if (badge && (badge.textContent === "New" || badge.textContent === "Reminder")) {
                badge.remove();
            }
        });

        updateUnreadCount();
    });
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

document.addEventListener("DOMContentLoaded", () => {
    initServicesDropdown();
    initMobileMenu();
    initFilterButtons();
    initSearch();
    initNotificationActions();
    initMarkAllAsRead();
    initFadeUpAnimation();
    updateUnreadCount();
    filterNotifications();
});