const cartCount = document.getElementById("cartCount");
const reportButtons = document.querySelectorAll("[data-report-type]");
const reportModal = document.getElementById("reportModal");
const toast = document.getElementById("toast");

const modalTitle = document.getElementById("modalTitle");
const modalSubtitle = document.getElementById("modalSubtitle");
const entityFieldWrap = document.getElementById("entityFieldWrap");
const entityLabel = document.getElementById("entityLabel");
const entityName = document.getElementById("entityName");
const issueReason = document.getElementById("issueReason");

const reportForm = document.getElementById("reportForm");
const userName = document.getElementById("userName");
const userPhone = document.getElementById("userPhone");
const issueMessage = document.getElementById("issueMessage");

let currentType = "technical";

const reasonOptions = {
  technical: [
    "Site bug",
    "Payment issue",
    "Login problem",
    "Account issue",
    "Page not working",
    "Other technical problem"
  ],
  clinic: [
    "Wrong clinic information",
    "Appointment problem",
    "Doctor issue",
    "Delay in service",
    "Treatment concern",
    "Other clinic issue"
  ],
  store: [
    "Wrong product information",
    "Delivery problem",
    "Missing item",
    "Bad store service",
    "Order issue",
    "Other store issue"
  ]
};

const clinicOptions = [
  "Paws Veterinary Clinic",
  "Happy Tails Clinic",
  "Pet Care Center",
  "Green Vet Clinic",
  "Royal Pets Clinic"
];

const storeOptions = [
  "Waffy Pet Store",
  "Pet World Store",
  "Happy Paws Shop",
  "Animal House Store",
  "Best Friends Market"
];

function getCart() {
  return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function updateCartBadge() {
  const cart = getCart();
  const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
  if (cartCount) cartCount.textContent = totalItems;
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

function fillReasonOptions(type) {
  issueReason.innerHTML = `<option value="">Select a reason</option>`;

  reasonOptions[type].forEach((reason) => {
    const option = document.createElement("option");
    option.value = reason;
    option.textContent = reason;
    issueReason.appendChild(option);
  });
}

function fillEntityOptions(type) {
  entityName.innerHTML = `<option value="">Select an option</option>`;

  const options = type === "clinic" ? clinicOptions : storeOptions;

  options.forEach((item) => {
    const option = document.createElement("option");
    option.value = item;
    option.textContent = item;
    entityName.appendChild(option);
  });
}

function openModal(type) {
  currentType = type;
  reportForm.reset();
  entityFieldWrap.classList.add("hidden");

  if (type === "technical") {
    modalTitle.textContent = "Technical Issue Report";
    modalSubtitle.textContent = "Tell us about the technical problem you faced.";
  }

  if (type === "clinic") {
    modalTitle.textContent = "Clinic Issue Report";
    modalSubtitle.textContent = "Please choose the clinic related to the issue.";
    entityFieldWrap.classList.remove("hidden");
    entityLabel.textContent = "Clinic Name";
    fillEntityOptions("clinic");
  }

  if (type === "store") {
    modalTitle.textContent = "Store Issue Report";
    modalSubtitle.textContent = "Please choose the store related to the issue.";
    entityFieldWrap.classList.remove("hidden");
    entityLabel.textContent = "Store Name";
    fillEntityOptions("store");
  }

  fillReasonOptions(type);
  reportModal.classList.remove("hidden");
  document.body.style.overflow = "hidden";
}

function closeModal() {
  reportModal.classList.add("hidden");
  document.body.style.overflow = "";
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

reportButtons.forEach((button) => {
  button.addEventListener("click", () => {
    openModal(button.dataset.reportType);
  });
});

document.addEventListener("click", (event) => {
  const closeTarget = event.target.closest("[data-close-modal]");
  if (closeTarget) {
    closeModal();
  }
});

document.addEventListener("keydown", (event) => {
  if (event.key === "Escape" && reportModal && !reportModal.classList.contains("hidden")) {
    closeModal();
  }
});

if (reportForm) {
  reportForm.addEventListener("submit", (event) => {
    event.preventDefault();

    const nameValue = userName.value.trim();
    const phoneValue = userPhone.value.trim();
    const reasonValue = issueReason.value.trim();
    const messageValue = issueMessage.value.trim();
    const entityValue = entityName.value.trim();

    if (!nameValue || !phoneValue || !reasonValue || !messageValue) {
      showToast("Please fill in all required fields.");
      return;
    }

    if ((currentType === "clinic" || currentType === "store") && !entityValue) {
      showToast(`Please select the ${currentType} name.`);
      return;
    }

    closeModal();
    showToast("We received your report successfully. Don’t worry, we’ll contact you soon.");
  });
}

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  initFadeUpAnimation();
  initDesktopServicesMenu();
  initMobileMenu();
});