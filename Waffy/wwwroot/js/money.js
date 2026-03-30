const cartCount = document.getElementById("cartCount");
const typeCards = document.querySelectorAll(".type-card");
const amountButtons = document.querySelectorAll(".amount-btn");
const paymentCards = document.querySelectorAll(".payment-card");
const animalCards = document.querySelectorAll(".animal-card");

const moneySection = document.getElementById("moneySection");
const foodSection = document.getElementById("foodSection");
const medicineSection = document.getElementById("medicineSection");
const suppliesSection = document.getElementById("suppliesSection");

const customAmountWrap = document.getElementById("customAmountWrap");
const customAmountInput = document.getElementById("customAmountInput");

const walletBox = document.getElementById("walletBox");
const cardBox = document.getElementById("cardBox");
const cliqBox = document.getElementById("cliqBox");

const walletSelectedAmount = document.getElementById("walletSelectedAmount");
const walletRemainingAmount = document.getElementById("walletRemainingAmount");

const donorName = document.getElementById("donorName");
const donorPhone = document.getElementById("donorPhone");
const submitDonationBtn = document.getElementById("submitDonationBtn");
const cancelBtn = document.getElementById("cancelBtn");
const toast = document.getElementById("toast");

const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

let selectedType = "money";
let selectedAmount = 10;
let selectedPayment = "wallet";
let selectedAnimal = "";

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
  }, 3200);
}

function updateTypeSections() {
  moneySection.classList.toggle("hidden", selectedType !== "money");
  foodSection.classList.toggle("hidden", selectedType !== "food");
  medicineSection.classList.toggle("hidden", selectedType !== "medicine");
  suppliesSection.classList.toggle("hidden", selectedType !== "supplies");
}

function updateWalletBox() {
  const walletBalance = 25;
  const amount = Number(selectedAmount) || 0;
  const remaining = Math.max(walletBalance - amount, 0);

  walletSelectedAmount.textContent = `${amount} JD`;
  walletRemainingAmount.textContent = `${remaining} JD`;
}

function updatePaymentSections() {
  walletBox.classList.toggle("hidden", selectedPayment !== "wallet");
  cardBox.classList.toggle("hidden", selectedPayment !== "card");
  cliqBox.classList.toggle("hidden", selectedPayment !== "cliq");
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

typeCards.forEach((card) => {
  card.addEventListener("click", () => {
    typeCards.forEach((item) => item.classList.remove("active"));
    card.classList.add("active");
    selectedType = card.dataset.type;
    updateTypeSections();
  });
});

amountButtons.forEach((button) => {
  button.addEventListener("click", () => {
    amountButtons.forEach((item) => item.classList.remove("active"));
    button.classList.add("active");

    const amount = button.dataset.amount;

    if (amount === "custom") {
      customAmountWrap.classList.remove("hidden");
      selectedAmount = Number(customAmountInput.value || 0);
    } else {
      customAmountWrap.classList.add("hidden");
      selectedAmount = Number(amount);
    }

    updateWalletBox();
  });
});

customAmountInput.addEventListener("input", () => {
  selectedAmount = Number(customAmountInput.value || 0);
  updateWalletBox();
});

paymentCards.forEach((card) => {
  card.addEventListener("click", () => {
    paymentCards.forEach((item) => item.classList.remove("active"));
    card.classList.add("active");
    selectedPayment = card.dataset.payment;
    updatePaymentSections();
  });
});

animalCards.forEach((card) => {
  card.addEventListener("click", () => {
    animalCards.forEach((item) => item.classList.remove("active"));
    card.classList.add("active");
    selectedAnimal = card.dataset.animalCard;
  });
});

submitDonationBtn.addEventListener("click", () => {
  const nameValue = donorName.value.trim();
  const phoneValue = donorPhone.value.trim();

  if (!nameValue || !phoneValue) {
    showToast("Please enter your name and phone number first.");
    return;
  }

  if (selectedType === "money" && (!selectedAmount || selectedAmount <= 0)) {
    showToast("Please choose a valid donation amount.");
    return;
  }

  if (selectedType === "food") {
    const foodType = document.getElementById("foodType").value.trim();
    const foodName = document.getElementById("foodName").value.trim();
    const foodAnimal = document.getElementById("foodAnimal").value.trim();

    if (!foodType || !foodName || !foodAnimal) {
      showToast("Please complete the food donation details.");
      return;
    }
  }

  if (selectedType === "medicine") {
    const medicineName = document.getElementById("medicineName").value.trim();
    const medicineType = document.getElementById("medicineType").value.trim();
    const medicineAnimal = document.getElementById("medicineAnimal").value.trim();

    if (!medicineName || !medicineType || !medicineAnimal) {
      showToast("Please complete the medicine donation details.");
      return;
    }
  }

  if (selectedType === "supplies") {
    const supplyName = document.getElementById("supplyName").value.trim();
    const supplyCategory = document.getElementById("supplyCategory").value.trim();
    const supplyAnimal = document.getElementById("supplyAnimal").value.trim();

    if (!supplyName || !supplyCategory || !supplyAnimal) {
      showToast("Please complete the supplies donation details.");
      return;
    }
  }

  showToast("Thank you for your kindness. Your donation has been prepared successfully.");
});

cancelBtn.addEventListener("click", () => {
  window.scrollTo({ top: 0, behavior: "smooth" });
});

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  updateTypeSections();
  updateWalletBox();
  updatePaymentSections();
  initFadeUpAnimation();
  initServicesDropdown();
  initMobileMenu();
});