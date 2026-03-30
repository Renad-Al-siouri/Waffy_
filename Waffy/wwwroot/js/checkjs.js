const checkoutItems = document.getElementById("checkoutItems");
const cartCount = document.getElementById("cartCount");
const summarySubtotal = document.getElementById("summarySubtotal");
const summaryShipping = document.getElementById("summaryShipping");
const summaryTotal = document.getElementById("summaryTotal");
const checkoutBtn = document.getElementById("checkoutBtn");
const successModal = document.getElementById("successModal");
const paymentRadios = document.querySelectorAll('input[name="paymentMethod"]');
const cardFields = document.getElementById("cardFields");
const backCartBtn = document.getElementById("backCartBtn");
const toast = document.getElementById("toast");

const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

const SHIPPING_FEE = 2;

function getCart() {
  return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function showToast(message) {
  if (!toast) return;

  toast.textContent = message;
  toast.classList.add("show");

  clearTimeout(showToast.timer);
  showToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 3000);
}

function updateCartBadge() {
  if (!cartCount) return;

  const cart = getCart();
  const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
  cartCount.textContent = totalItems;
}

function capitalize(text) {
  if (!text) return "";
  return text.charAt(0).toUpperCase() + text.slice(1);
}

function renderCheckoutItems() {
  if (!checkoutItems) return;

  const cart = getCart();

  if (!cart.length) {
    checkoutItems.innerHTML = `
      <div class="empty-cart">
        <h3>Your cart is empty</h3>
        <p>Add some products from the supplies page first.</p>
      </div>
    `;
    summarySubtotal.textContent = "0.00 JD";
    summaryShipping.textContent = "0.00 JD";
    summaryTotal.textContent = "0.00 JD";
    return;
  }

  checkoutItems.innerHTML = cart.map((item) => `
    <div class="checkout-item">
      <div class="checkout-item-image">
        <img src="${item.image}" alt="${item.name}">
      </div>

      <div class="checkout-item-info">
        <h4>${item.name}</h4>
        <p>Qty: ${item.quantity}</p>
        <p>${capitalize(item.category)}</p>
      </div>

      <div class="checkout-item-price">
        ${(item.price * item.quantity).toFixed(2)} JD
      </div>
    </div>
  `).join("");

  const subtotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
  const total = subtotal + SHIPPING_FEE;

  summarySubtotal.textContent = `${subtotal.toFixed(2)} JD`;
  summaryShipping.textContent = `${SHIPPING_FEE.toFixed(2)} JD`;
  summaryTotal.textContent = `${total.toFixed(2)} JD`;
}

function validateCheckoutForm() {
  const fullName = document.getElementById("fullName")?.value.trim();
  const email = document.getElementById("email")?.value.trim();
  const phone = document.getElementById("phone")?.value.trim();
  const address = document.getElementById("address")?.value.trim();
  const postalCode = document.getElementById("postalCode")?.value.trim();
  const city = document.getElementById("city")?.value.trim();
  const country = document.getElementById("country")?.value.trim();

  if (!fullName || !email || !phone || !address || !postalCode || !city || !country) {
    showToast("Please fill in all required personal and shipping information.");
    return false;
  }

  const selectedMethod = document.querySelector('input[name="paymentMethod"]:checked')?.value;

  if (selectedMethod === "card") {
    const cardholderName = document.getElementById("cardholderName")?.value.trim();
    const cardNumber = document.getElementById("cardNumber")?.value.trim();
    const expDate = document.getElementById("expDate")?.value.trim();
    const cvc = document.getElementById("cvc")?.value.trim();

    if (!cardholderName || !cardNumber || !expDate || !cvc) {
      showToast("Please complete all card payment fields.");
      return false;
    }
  }

  return true;
}

function toggleCardFields() {
  if (!cardFields) return;

  const selectedMethod = document.querySelector('input[name="paymentMethod"]:checked')?.value;

  if (selectedMethod === "card") {
    cardFields.classList.remove("hidden");
  } else {
    cardFields.classList.add("hidden");
  }
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

function initServicesDropdown() {
  if (!servicesToggle || !servicesDropdown) return;

  servicesToggle.addEventListener("click", (e) => {
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

paymentRadios.forEach((radio) => {
  radio.addEventListener("change", toggleCardFields);
});

if (checkoutBtn) {
  checkoutBtn.addEventListener("click", () => {
    const cart = getCart();

    if (!cart.length) {
      showToast("Your cart is empty.");
      window.location.href = "cart.html";
      return;
    }

    if (!validateCheckoutForm()) return;

    localStorage.setItem("waffyPaymentDone", "true");

    if (successModal) {
      successModal.classList.remove("hidden");
    }
  });
}

if (backCartBtn) {
  backCartBtn.addEventListener("click", () => {
    window.location.href = "cart.html";
  });
}

const successOverlay = document.querySelector(".success-overlay");
if (successOverlay && successModal) {
  successOverlay.addEventListener("click", () => {
    successModal.classList.add("hidden");
  });
}

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  renderCheckoutItems();
  toggleCardFields();
  initFadeUpAnimation();
  initServicesDropdown();
  initMobileMenu();
});