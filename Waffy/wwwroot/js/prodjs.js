/* ============================================================
   prodjs.js — Waffy Products Page  (UI only)
   ❌ لا يوجد products array
   ❌ لا يوجد renderProducts()
   ✅ المنتجات تأتي من السيرفر عبر Razor
   ✅ هنا فقط: Cart + Qty + Navbar + Fade
   ============================================================ */

(function () {
    "use strict";

    /* ═══════════════════════════════════════════
       CART — localStorage
    ═══════════════════════════════════════════ */
    function getCart() {
        try { return JSON.parse(localStorage.getItem("waffyCart") || "[]"); }
        catch { return []; }
    }

    function saveCart(cart) {
        localStorage.setItem("waffyCart", JSON.stringify(cart));
    }

    /* ═══════════════════════════════════════════
       TOAST
    ═══════════════════════════════════════════ */
    var _toastTimer;
    function showToast(msg) {
        var t = document.getElementById("toast");
        if (!t) return;
        t.textContent = msg;
        t.classList.add("show");
        clearTimeout(_toastTimer);
        _toastTimer = setTimeout(function () {
            t.classList.remove("show");
        }, 2500);
    }

    /* ═══════════════════════════════════════════
       QTY HELPERS  (per card)
    ═══════════════════════════════════════════ */
    function getQtyEl(card) {
        return card.querySelector(".qty-value");
    }

    function getQty(card) {
        var el = getQtyEl(card);
        return el ? Math.max(1, parseInt(el.textContent, 10) || 1) : 1;
    }

    function setQty(card, n) {
        var el = getQtyEl(card);
        if (el) el.textContent = Math.max(1, n);
    }

    /* ═══════════════════════════════════════════
       ADD TO CART
    ═══════════════════════════════════════════ */
    function addToCart(card) {
        var id = parseInt(card.dataset.id || "0", 10);
        var name = card.dataset.name || "Product";
        var price = parseFloat(card.dataset.price || "0");
        var image = card.dataset.image || "";
        var qty = getQty(card);

        if (!id) return;

        var cart = getCart();
        var existing = cart.find(function (i) { return i.id === id; });

        if (existing) {
            existing.quantity += qty;
        } else {
            cart.push({ id: id, name: name, price: price, image: image, quantity: qty });
        }

        saveCart(cart);
        setQty(card, 1);
        showToast(qty + " \u00d7 \u201c" + name + "\u201d added to cart \uD83D\uDED2");
    }

    /* ═══════════════════════════════════════════
       EVENT DELEGATION on productsGrid
    ═══════════════════════════════════════════ */
    var grid = document.getElementById("productsGrid");
    if (grid) {
        grid.addEventListener("click", function (e) {
            var card = e.target.closest(".product-card");
            if (!card) return;

            var qtyBtn = e.target.closest(".qty-btn");
            var cartBtn = e.target.closest(".add-cart-btn");

            if (qtyBtn) {
                var action = qtyBtn.dataset.action;
                var current = getQty(card);
                setQty(card, action === "increase" ? current + 1 : current - 1);
                return;
            }

            if (cartBtn) {
                addToCart(card);
            }
        });
    }

    /* ═══════════════════════════════════════════
       SEARCH — debounce 500ms then form submit
    ═══════════════════════════════════════════ */
    var searchInput = document.getElementById("searchInput");
    var filterForm = document.getElementById("filterForm");

    if (searchInput && filterForm) {
        var _searchTimer;
        searchInput.addEventListener("input", function () {
            clearTimeout(_searchTimer);
            _searchTimer = setTimeout(function () {
                filterForm.submit();
            }, 500);
        });
    }

    /* ═══════════════════════════════════════════
       NAVBAR — Desktop Dropdown
    ═══════════════════════════════════════════ */
    var dropdown = document.getElementById("servicesDropdown");
    var toggle = document.getElementById("servicesToggle");

    if (dropdown && toggle) {
        toggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropdown.classList.toggle("open");
        });
        document.addEventListener("click", function (e) {
            if (dropdown && !dropdown.contains(e.target)) {
                dropdown.classList.remove("open");
            }
        });
    }

    /* ═══════════════════════════════════════════
       NAVBAR — Mobile Menu
    ═══════════════════════════════════════════ */
    var menuBtn = document.getElementById("menuBtn");
    var navLinks = document.getElementById("navLinks");

    if (menuBtn && navLinks) {
        menuBtn.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            navLinks.classList.toggle("open");
        });
        document.addEventListener("click", function (e) {
            if (navLinks &&
                !navLinks.contains(e.target) &&
                menuBtn && !menuBtn.contains(e.target)) {
                navLinks.classList.remove("open");
            }
        });
    }

    var mobBlock = document.getElementById("mobileServicesBlock");
    var mobToggle = document.getElementById("mobileServicesToggle");

    if (mobBlock && mobToggle) {
        mobToggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            mobBlock.classList.toggle("open");
        });
    }

    /* ═══════════════════════════════════════════
       FADE-UP ANIMATION
    ═══════════════════════════════════════════ */
    var fadeEls = document.querySelectorAll(".fade-up");

    if ("IntersectionObserver" in window) {
        var obs = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("show");
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        fadeEls.forEach(function (el) { obs.observe(el); });
    } else {
        fadeEls.forEach(function (el) { el.classList.add("show"); });
    }

})();