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
    let selectedSeverity = "Medium";

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
            // ✅ تحديث الـ hidden inputs
            var hLat = document.getElementById('hLatitude');
            var hLng = document.getElementById('hLongitude');
            if (hLat) hLat.value = e.latlng.lat.toFixed(6);
            if (hLng) hLng.value = e.latlng.lng.toFixed(6);
        });
    }

    function initChoiceButtons() {
        animalChoiceButtons.forEach((btn) => {
            btn.addEventListener("click", () => {
                animalChoiceButtons.forEach((b) => b.classList.remove("active"));
                btn.classList.add("active");
                selectedAnimal = btn.dataset.animal;
                // ✅ داخل animal listener
                var hA = document.getElementById('hAnimalType');
                if (hA) hA.value = btn.dataset.animal;
            });
        });

        problemChoiceButtons.forEach((btn) => {
            btn.addEventListener("click", () => {
                problemChoiceButtons.forEach((b) => b.classList.remove("active"));
                btn.classList.add("active");
                selectedProblem = btn.dataset.problem;
                // ✅ داخل problem listener
                var hP = document.getElementById('hProblemType');
                if (hP) hP.value = btn.dataset.problem;
            });
        });

        severityButtons.forEach((btn) => {
            btn.addEventListener("click", () => {
                severityButtons.forEach((b) => b.classList.remove("active"));
                btn.classList.add("active");
                selectedSeverity = btn.dataset.severity;
                // ✅ داخل severity listener
                var hS = document.getElementById('hSeverity');
                if (hS) hS.value = btn.dataset.severity;
            });
        });
    }

    function initSubmitReport() {
        if (!submitReportBtn) return;

        submitReportBtn.addEventListener("click", () => {

            // ✅ تحقق من تسجيل الدخول — لو الفورم مش موجود = غير مسجل
            var form = document.getElementById('dangerForm');
            if (!form) {
                showToast("🔒 Please log in first to submit a report.");
                setTimeout(() => { window.location.href = '/Account/Login'; }, 2000);
                return;
            }

            // ✅ اقرأ من hidden inputs مباشرةً — أضمن من المتغيرات
            var animalType = (document.getElementById('hAnimalType') || {}).value || selectedAnimal;
            var problemType = (document.getElementById('hProblemType') || {}).value || selectedProblem;
            var severity = (document.getElementById('hSeverity') || {}).value || selectedSeverity;

            var cityInput = document.getElementById("cityInput");
            var streetInput = document.getElementById("streetInput");
            var descriptionInput = document.getElementById("descriptionInput");

            var city = cityInput ? cityInput.value.trim() : "";
            var street = streetInput ? streetInput.value.trim() : "";
            var description = descriptionInput ? descriptionInput.value.trim() : "";

            var lat = (document.getElementById('hLatitude') || {}).value || "";
            var lng = (document.getElementById('hLongitude') || {}).value || "";

            if (!animalType) {
                showToast("Please choose the animal type.");
                return;
            }
            if (!problemType) {
                showToast("Please choose the problem type.");
                return;
            }
            if (!city) {
                showToast("Please select the city.");
                return;
            }
            if (!description) {
                showToast("Please enter a description.");
                return;
            }
            if (!lat || !lng) {
                showToast("Please choose the exact location on the map.");
                return;
            }

            // ✅ ملء locationText
            // ✅ ملء كل الحقول قبل الإرسال
            var hLoc = document.getElementById('hLocationText');
            var hCity = document.getElementById('hCity');
            var hArea = document.getElementById('hArea');
            var hDesc = document.getElementById('hDescription');

            if (hLoc) hLoc.value = street;
            if (hCity) hCity.value = city;
            if (hArea) hArea.value = street;
            if (hDesc) hDesc.value = description;

            // ✅ إرسال الفورم
            form.submit();
        });
    }

    updateCartBadge();
    initServicesDropdown();
    initMobileMenu();
    initReportMap();
    initChoiceButtons();
    initSubmitReport();
});