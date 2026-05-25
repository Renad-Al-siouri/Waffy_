/**
 * ═══════════════════════════════════════════════════════
 * Waffy — shared-navbar.js
 * File path: wwwroot/js/shared-navbar.js
 *
 * Handles all navbar interactions across every page:
 *   • Hamburger menu open / close
 *   • Services dropdown toggle
 *   • Mobile services accordion
 *   • Close on outside click
 *   • Close on window resize
 * ═══════════════════════════════════════════════════════
 */
(function () {
    "use strict";

    const MOBILE_BREAKPOINT = 1188;

    // Cache elements
    const menuBtn = document.getElementById("menuBtn");
    const navLinks = document.getElementById("navLinks");
    const servicesToggle = document.getElementById("servicesToggle");
    const servicesDropdown = document.getElementById("servicesDropdown");
    const mobileServicesToggle = document.getElementById("mobileServicesToggle");
    const mobileServicesBlock = document.getElementById("mobileServicesBlock");

    // ── Hamburger ──────────────────────────────────────────
    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            navLinks.classList.toggle("open");
        });
    }

    // ── Desktop services dropdown ──────────────────────────
    if (servicesToggle && servicesDropdown) {
        servicesToggle.addEventListener("click", function (e) {
            e.stopPropagation();
            servicesDropdown.classList.toggle("open");
        });
    }

    // ── Mobile services accordion ──────────────────────────
    if (mobileServicesToggle && mobileServicesBlock) {
        mobileServicesToggle.addEventListener("click", function (e) {
            e.stopPropagation();
            mobileServicesBlock.classList.toggle("open");
        });
    }

    // ── Close drawer when a link inside it is clicked ──────
    if (navLinks) {
        navLinks.querySelectorAll(".nav-link, .dropdown-link").forEach(function (link) {
            link.addEventListener("click", function () {
                if (window.innerWidth <= MOBILE_BREAKPOINT) {
                    closeMobileMenu();
                }
            });
        });
    }

    // ── Close on outside click ─────────────────────────────
    document.addEventListener("click", function (e) {
        // Close desktop services dropdown
        if (servicesDropdown && !servicesDropdown.contains(e.target)) {
            servicesDropdown.classList.remove("open");
        }

        // Close mobile drawer
        if (
            navLinks &&
            menuBtn &&
            !navLinks.contains(e.target) &&
            !menuBtn.contains(e.target)
        ) {
            closeMobileMenu();
        }
    });

    // ── Close on resize to desktop ─────────────────────────
    window.addEventListener("resize", function () {
        if (window.innerWidth > MOBILE_BREAKPOINT) {
            closeMobileMenu();
        }
    });

    // ── Helper ─────────────────────────────────────────────
    function closeMobileMenu() {
        if (navLinks) navLinks.classList.remove("open");
        if (mobileServicesBlock) mobileServicesBlock.classList.remove("open");
    }

})();