const faqGroups = document.querySelectorAll(".faq-group");
const faqQuestions = document.querySelectorAll(".faq-question");
const cartCount = document.getElementById("cartCount");

const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

function getCart() {
  return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function updateCartBadge() {
  if (!cartCount) return;

  const cart = getCart();
  const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
  cartCount.textContent = totalItems;
}

function initFAQGroups() {
  faqGroups.forEach((group) => {
    const groupTitle = group.querySelector(".faq-group-title");
    const groupContent = group.querySelector(".faq-group-content");

    if (!groupTitle || !groupContent) return;

    groupTitle.addEventListener("click", () => {
      const isOpen = group.classList.contains("active");

      faqGroups.forEach((item) => {
        item.classList.remove("active");
        item.querySelector(".faq-group-content")?.classList.remove("open");

        item.querySelectorAll(".faq-item").forEach((faqItem) => {
          faqItem.classList.remove("active");
          faqItem.querySelector(".faq-answer")?.classList.remove("open");
        });
      });

      if (!isOpen) {
        group.classList.add("active");
        groupContent.classList.add("open");

        const firstItem = groupContent.querySelector(".faq-item");
        if (firstItem) {
          firstItem.classList.add("active");
          firstItem.querySelector(".faq-answer")?.classList.add("open");
        }
      }
    });
  });
}

function initFAQQuestions() {
  faqQuestions.forEach((question) => {
    question.addEventListener("click", () => {
      const item = question.closest(".faq-item");
      const answer = item?.querySelector(".faq-answer");
      const isOpen = item?.classList.contains("active");
      const groupContent = question.closest(".faq-group-content");
      const allItems = groupContent?.querySelectorAll(".faq-item") || [];

      allItems.forEach((faqItem) => {
        faqItem.classList.remove("active");
        faqItem.querySelector(".faq-answer")?.classList.remove("open");
      });

      if (!isOpen && item && answer) {
        item.classList.add("active");
        answer.classList.add("open");
      }
    });
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

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  initFAQGroups();
  initFAQQuestions();
  initFadeUpAnimation();
  initServicesDropdown();
  initMobileMenu();
});