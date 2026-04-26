/* ============================================================
   stores.js — UI only
   البحث والفلترة تتم في الباك عبر GET form
   هنا فقط: dropdown + mobile menu + fade animation
   + auto-submit البحث عند الكتابة (debounce)
   ============================================================ */

(function () {
    "use strict";

    /* ── Dropdown ────────────────────────────────────────── */
    const dropdown = document.getElementById("servicesDropdown");
    const toggle = document.getElementById("servicesToggle");

    if (dropdown && toggle) {
        toggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropdown.classList.toggle("open");
        });
        document.addEventListener("click", function (e) {
            if (!dropdown.contains(e.target)) dropdown.classList.remove("open");
        });
    }

    /* ── Mobile Menu ─────────────────────────────────────── */
    const menuBtn = document.getElementById("menuBtn");
    const navLinks = document.getElementById("navLinks");

    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            navLinks.classList.toggle("open");
        });
        document.addEventListener("click", function (e) {
            if (navLinks && !navLinks.contains(e.target) && !menuBtn.contains(e.target)) {
                navLinks.classList.remove("open");
            }
        });
    }

    const mobileBlock = document.getElementById("mobileServicesBlock");
    const mobileToggle = document.getElementById("mobileServicesToggle");
    if (mobileBlock && mobileToggle) {
        mobileToggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            mobileBlock.classList.toggle("open");
        });
    }

    /* ── Auto-submit البحث (debounce 500ms) ──────────────── */
    const searchInput = document.getElementById("storeSearch");
    const filterForm = document.getElementById("storeFilterForm");

    if (searchInput && filterForm) {
        let debounceTimer;
        searchInput.addEventListener("input", function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(function () {
                filterForm.submit();
            }, 500);
        });
    }

    /* ── Fade-up Animation (CSS Intersection Observer) ───── */
    const fadeEls = document.querySelectorAll(".fade-up");
    if ("IntersectionObserver" in window) {
        const obs = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("show");
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });
        fadeEls.forEach(function (el) { obs.observe(el); });
    } else {
        fadeEls.forEach(function (el) { el.classList.add("show"); });
    }

})();