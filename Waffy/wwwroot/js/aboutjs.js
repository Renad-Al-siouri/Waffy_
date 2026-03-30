function getCart() {
  return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function updateCartBadge() {
  const cartCount = document.getElementById("cartCount");
  if (!cartCount) return;

  const cart = getCart();
  const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
  cartCount.textContent = totalItems;
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

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  initFadeUpAnimation();
  initDesktopServicesMenu();
  initMobileMenu();
});