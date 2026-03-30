document.addEventListener("DOMContentLoaded", () => {
  const toast = document.getElementById("toast");
  const submitReportBtn = document.getElementById("submitReportBtn");
  const animalChoiceButtons = document.querySelectorAll("[data-animal]");
  const problemChoiceButtons = document.querySelectorAll("[data-problem]");
  const severityButtons = document.querySelectorAll("[data-severity]");

  const servicesDropdown = document.getElementById("servicesDropdown");
  const servicesToggle = document.getElementById("servicesToggle");
  const menuBtn = document.getElementById("menuBtn");
  const navLinks = document.getElementById("navLinks");
  const mobileServicesBlock = document.getElementById("mobileServicesBlock");
  const mobileServicesToggle = document.getElementById("mobileServicesToggle");
  const cartCount = document.getElementById("cartCount");

  let reportMap;
  let reportMarker = null;
  let selectedReportLocation = null;
  let selectedAnimal = "";
  let selectedProblem = "";
  let selectedSeverity = "medium";

  function getCart() {
    return JSON.parse(localStorage.getItem("waffyCart") || "[]");
  }

  function updateCartBadge() {
    if (!cartCount) return;
    const cart = getCart();
    const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
    cartCount.textContent = totalItems;
  }

  function showToast(message) {
    if (!toast) return;
    toast.textContent = message;
    toast.classList.add("show");

    clearTimeout(showToast.timer);
    showToast.timer = setTimeout(() => {
      toast.classList.remove("show");
    }, 2800);
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

  function initReportMap() {
    const reportMapElement = document.getElementById("reportMap");
    if (!reportMapElement) return;

    reportMap = L.map("reportMap", {
      zoomControl: true
    }).setView([31.9539, 35.9106], 12);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      attribution: "&copy; OpenStreetMap contributors"
    }).addTo(reportMap);

    reportMap.on("click", (e) => {
      selectedReportLocation = e.latlng;

      if (reportMarker) {
        reportMarker.setLatLng(e.latlng);
      } else {
        reportMarker = L.marker(e.latlng).addTo(reportMap);
      }
    });
  }

  function initChoiceButtons() {
    animalChoiceButtons.forEach((btn) => {
      btn.addEventListener("click", () => {
        animalChoiceButtons.forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        selectedAnimal = btn.dataset.animal;
      });
    });

    problemChoiceButtons.forEach((btn) => {
      btn.addEventListener("click", () => {
        problemChoiceButtons.forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        selectedProblem = btn.dataset.problem;
      });
    });

    severityButtons.forEach((btn) => {
      btn.addEventListener("click", () => {
        severityButtons.forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        selectedSeverity = btn.dataset.severity;
      });
    });
  }

  function initSubmitReport() {
    if (!submitReportBtn) return;

    submitReportBtn.addEventListener("click", () => {
      const cityInput = document.getElementById("cityInput");
      const streetInput = document.getElementById("streetInput");
      const descriptionInput = document.getElementById("descriptionInput");

      const city = cityInput ? cityInput.value.trim() : "";
      const street = streetInput ? streetInput.value.trim() : "";
      const description = descriptionInput ? descriptionInput.value.trim() : "";

      if (!selectedAnimal) {
        showToast("Please choose the animal type.");
        return;
      }

      if (!selectedProblem) {
        showToast("Please choose the problem type.");
        return;
      }

      if (!city || !street || !description) {
        showToast("Please fill in the required location and description fields.");
        return;
      }

      if (!selectedReportLocation) {
        showToast("Please choose the exact location on the map.");
        return;
      }

      showToast("Your report has been sent successfully.");
    });
  }

  updateCartBadge();
  initServicesDropdown();
  initMobileMenu();
  initReportMap();
  initChoiceButtons();
  initSubmitReport();
});