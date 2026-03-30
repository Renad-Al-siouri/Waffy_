const cartItemsContainer = document.getElementById("cartItems");
const cartCount = document.getElementById("cartCount");
const summaryItems = document.getElementById("summaryItems");
const summarySubtotal = document.getElementById("summarySubtotal");
const summaryFee = document.getElementById("summaryFee");
const summaryTotal = document.getElementById("summaryTotal");
const clearCartBtn = document.getElementById("clearCartBtn");
const checkoutBtn = document.getElementById("checkoutBtn");
const toast = document.getElementById("toast");
const paymentNotice = document.getElementById("paymentNotice");

const SERVICE_FEE = 1.5;

function getCart() {
  return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function saveCart(cart) {
  localStorage.setItem("waffyCart", JSON.stringify(cart));
}

function showToast(message) {
  toast.textContent = message;
  toast.classList.add("show");

  clearTimeout(showToast.timer);
  showToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 3000);
}

function updateCartBadge() {
  const cart = getCart();
  const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
  if (cartCount) cartCount.textContent = totalItems;
}

function checkPaymentNotice() {
  const paid = localStorage.getItem("waffyPaymentDone");

  if (paid === "true") {
    paymentNotice.classList.remove("hidden");
    localStorage.removeItem("waffyPaymentDone");
  } else {
    paymentNotice.classList.add("hidden");
  }
}

function renderSummary(cart) {
  const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
  const subtotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
  const total = cart.length ? subtotal + SERVICE_FEE : 0;

  summaryItems.textContent = totalItems;
  summarySubtotal.textContent = `${subtotal.toFixed(2)} JD`;
  summaryFee.textContent = cart.length ? `${SERVICE_FEE.toFixed(2)} JD` : `0.00 JD`;
  summaryTotal.textContent = `${total.toFixed(2)} JD`;
}

function capitalize(text) {
  if (!text) return "";
  return text.charAt(0).toUpperCase() + text.slice(1);
}

function renderCart() {
  const cart = getCart();

  if (!cart.length) {
    cartItemsContainer.innerHTML = `
      <div class="empty-cart">
        <h3>Your cart is empty</h3>
        <p>Add some lovely products from the supplies page and come back here.</p>
      </div>
    `;
    renderSummary(cart);
    updateCartBadge();
    return;
  }

  cartItemsContainer.innerHTML = cart.map((item) => `
    <article class="cart-item">
      <div class="cart-item-image">
        <img src="${item.image}" alt="${item.name}">
      </div>

      <div class="cart-item-info">
        <h3>${item.name}</h3>
        <p><strong>Category:</strong> ${capitalize(item.category)}</p>
        <p><strong>Unit Price:</strong> ${item.price.toFixed(2)} JD</p>

        <div class="cart-item-meta">
          <div class="qty-mini">
            <button class="qty-mini-btn" data-action="decrease" data-id="${item.id}">-</button>
            <span class="qty-mini-value">${item.quantity}</span>
            <button class="qty-mini-btn" data-action="increase" data-id="${item.id}">+</button>
          </div>

          <span class="cart-item-price">
            Total: ${(item.price * item.quantity).toFixed(2)} JD
          </span>
        </div>
      </div>

      <div class="cart-item-actions">
        <button class="remove-btn" data-action="remove" data-id="${item.id}">
          Remove
        </button>
      </div>
    </article>
  `).join("");

  renderSummary(cart);
  updateCartBadge();
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

function initDesktopServicesMenu() {
  const dropdown = document.getElementById("servicesDropdown");
  const toggle = document.getElementById("servicesToggle");

  if (!dropdown || !toggle) return;

  toggle.addEventListener("click", (e) => {
    e.stopPropagation();
    dropdown.classList.toggle("open");
  });

  document.addEventListener("click", (e) => {
    if (!dropdown.contains(e.target)) {
      dropdown.classList.remove("open");
    }
  });
}

function initMobileMenu() {
  const menuBtn = document.getElementById("menuBtn");
  const navLinks = document.getElementById("navLinks");
  const mobileServicesBlock = document.getElementById("mobileServicesBlock");
  const mobileServicesToggle = document.getElementById("mobileServicesToggle");

  if (menuBtn && navLinks) {
    menuBtn.addEventListener("click", (e) => {
      e.stopPropagation();
      navLinks.classList.toggle("open");
    });

    document.addEventListener("click", (e) => {
      if (!navLinks.contains(e.target) && !menuBtn.contains(e.target)) {
        navLinks.classList.remove("open");
      }
    });
  }

  if (mobileServicesBlock && mobileServicesToggle) {
    mobileServicesToggle.addEventListener("click", () => {
      mobileServicesBlock.classList.toggle("open");
    });
  }
}

document.addEventListener("click", (e) => {
  const btn = e.target.closest("[data-action]");
  if (!btn) return;

  const action = btn.dataset.action;
  const id = Number(btn.dataset.id);
  const cart = getCart();
  const item = cart.find((product) => product.id === id);

  if (action === "increase" && item) {
    item.quantity += 1;
    saveCart(cart);
    renderCart();
    return;
  }

  if (action === "decrease" && item) {
    item.quantity -= 1;

    if (item.quantity <= 0) {
      const updatedCart = cart.filter((product) => product.id !== id);
      saveCart(updatedCart);
    } else {
      saveCart(cart);
    }

    renderCart();
    return;
  }

  if (action === "remove") {
    const updatedCart = cart.filter((product) => product.id !== id);
    saveCart(updatedCart);
    renderCart();
    showToast("Product removed from cart.");
  }
});

clearCartBtn.addEventListener("click", () => {
  const cart = getCart();

  if (!cart.length) {
    showToast("Your cart is already empty.");
    return;
  }

  localStorage.removeItem("waffyCart");
  renderCart();
  showToast("Your cart has been cleared.");
});

checkoutBtn.addEventListener("click", () => {
  const cart = getCart();

  if (!cart.length) {
    showToast("Your cart is empty. Add some products first.");
    return;
  }

  window.location.href = "check.html";
});

document.addEventListener("DOMContentLoaded", () => {
  renderCart();
  checkPaymentNotice();
  initFadeUpAnimation();
  initDesktopServicesMenu();
  initMobileMenu();
});