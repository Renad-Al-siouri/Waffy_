const cartCount = document.getElementById("cartCount");
const viewMoreBtn = document.getElementById("viewMoreBtn");
const petsGrid = document.getElementById("petsGrid");

const flowModal = document.getElementById("flowModal");
const abuseModal = document.getElementById("abuseModal");
const flowSteps = document.querySelectorAll(".flow-step");

const toast = document.getElementById("toast");
const countdownText = document.getElementById("countdownText");

const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

const submitAbuseBtn = document.getElementById("submitAbuseBtn");

let map;
let marker;
let selectedLatLng = null;
let currentPetId = null;
let countdownInterval = null;

const extraPets = [
    {
        name: "Milo",
        status: "Done",
        details: ["Golden Mix, Male", "Friendly and active", "1.5 years old", "Blue collar"],
        location: "📍 Dabouq, Amman",
        phone: "📞 0791112233",
        image: "../Images/lost/Golden.png"
    },
    {
        name: "Nina",
        status: "Done",
        details: ["Small Cat, Female", "White and gray", "2 years old", "Very calm"],
        location: "📍 Abdoun, Amman",
        phone: "📞 0793332211",
        image: "../Images/lost/Small.png"
    }
];

let shownExtra = false;

function getCart() {
    return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function updateCartBadge() {
    const cart = getCart();
    const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);

    if (cartCount) {
        cartCount.textContent = totalItems;
    }
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

function createPetCard(pet, id = "") {
    return `
    <article class="pet-card" ${id ? `data-pet-id="${id}"` : ""}>
      <div class="pet-top">
        <img src="${pet.image}" alt="${pet.name}">
        <div class="pet-info">
          <div class="pet-head">
            <h3>${pet.name}</h3>
            <button class="status-btn" data-open-flow="${id || 999}">${pet.status}</button>
          </div>
          ${pet.details.map(item => `<p>${item}</p>`).join("")}
        </div>
      </div>
      <div class="pet-location">${pet.location}</div>
      <div class="pet-phone">${pet.phone}</div>
    </article>
  `;
}

function showStep(stepNumber) {
    flowSteps.forEach((step) => {
        step.classList.remove("active");
    });

    const activeStep = document.querySelector(`.flow-step[data-step="${stepNumber}"]`);
    if (activeStep) {
        activeStep.classList.add("active");
    }

    if (stepNumber === 2) {
        setTimeout(initMapIfNeeded, 100);
    }

    if (stepNumber === 3) {
        startCountdown();
    } else {
        stopCountdown();
    }
}

function resetFlowFields() {
    const ids = [
        "finderName",
        "finderPhone",
        "secretAnswer",
        "animalColor",
        "dateFound",
        "timeFound",
        "addressFound",
        "animalDescription"
    ];

    ids.forEach((id) => {
        const el = document.getElementById(id);
        if (el) el.value = "";
    });

    const animalType = document.getElementById("animalType");
    if (animalType) animalType.value = "";

    const animalPhoto = document.getElementById("animalPhoto");
    if (animalPhoto) animalPhoto.value = "";

    selectedLatLng = null;

    if (marker && map) {
        map.removeLayer(marker);
        marker = null;
    }
}

function openFlow(petId) {
    currentPetId = Number(petId);
    resetFlowFields();

    if (flowModal) {
        flowModal.classList.remove("hidden");
        showStep(1);
    }
}

function closeFlow() {
    if (flowModal) {
        flowModal.classList.add("hidden");
    }
    stopCountdown();
}

function openAbuseModal() {
    if (abuseModal) {
        abuseModal.classList.remove("hidden");
    }
}

function closeAbuseModal() {
    if (abuseModal) {
        abuseModal.classList.add("hidden");
    }
}

function initMapIfNeeded() {
    const mapElement = document.getElementById("map");
    if (!mapElement || typeof L === "undefined") return;

    if (!map) {
        map = L.map("map").setView([31.9539, 35.9106], 11);

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors"
        }).addTo(map);

        map.on("click", (e) => {
            selectedLatLng = e.latlng;

            if (marker) {
                marker.setLatLng(e.latlng);
            } else {
                marker = L.marker(e.latlng).addTo(map);
            }
        });
    }

    setTimeout(() => {
        if (map) map.invalidateSize();
    }, 100);
}

function validateStep2() {
    const animalType = document.getElementById("animalType")?.value.trim() || "";
    const dateFound = document.getElementById("dateFound")?.value.trim() || "";
    const addressFound = document.getElementById("addressFound")?.value.trim() || "";
    const animalDescription = document.getElementById("animalDescription")?.value.trim() || "";

    if (!animalType || !dateFound || !addressFound || !animalDescription) {
        showToast("Please fill in the required found animal information.");
        return false;
    }

    if (!selectedLatLng) {
        showToast("Please choose the location on the map.");
        return false;
    }

    return true;
}

function goToVerificationStep() {
    const finderName = document.getElementById("finderName")?.value.trim() || "";
    const finderPhone = document.getElementById("finderPhone")?.value.trim() || "";
    const secretAnswer = document.getElementById("secretAnswer")?.value.trim() || "";

    if (!finderName || !finderPhone || !secretAnswer) {
        showToast("Please enter your full name, phone number, and answer the secret question.");
        return;
    }

    showStep(4);
}

function startCountdown() {
    stopCountdown();

    let totalSeconds = 47 * 3600 + 51 * 60 + 48;

    function tick() {
        const hours = String(Math.floor(totalSeconds / 3600)).padStart(2, "0");
        const minutes = String(Math.floor((totalSeconds % 3600) / 60)).padStart(2, "0");
        const seconds = String(totalSeconds % 60).padStart(2, "0");

        if (countdownText) {
            countdownText.textContent = `${hours}:${minutes}:${seconds}`;
        }

        if (totalSeconds > 0) {
            totalSeconds -= 1;
        }
    }

    tick();
    countdownInterval = setInterval(tick, 1000);
}

function stopCountdown() {
    if (countdownInterval) {
        clearInterval(countdownInterval);
        countdownInterval = null;
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

function injectProfileLostPet() {
    const savedPet = localStorage.getItem("waffyLostPetFromProfile");
    if (!savedPet || !petsGrid) return;

    try {
        const pet = JSON.parse(savedPet);

        const profilePet = {
            name: pet.name || "My Lost Pet",
            status: "Done",
            details: [
                pet.breed || "Unknown breed",
                pet.color || "Unknown color",
                pet.age || "Unknown age",
                pet.note || "Missing pet"
            ],
            location: `📍 ${pet.location || "Unknown location"}`,
            phone: `📞 ${pet.phone || "No phone"}`,
            image: pet.image || "../Images/lost/Beagle.png"
        };

        petsGrid.insertAdjacentHTML("afterbegin", createPetCard(profilePet, "profile-lost"));
    } catch (error) {
        console.error("Invalid lost pet profile data", error);
    }
}

document.addEventListener("click", (e) => {
    const openFlowBtn = e.target.closest("[data-open-flow]");
    const nextBtn = e.target.closest("[data-next-step]");
    const prevBtn = e.target.closest("[data-prev-step]");
    const closeFlowBtn = e.target.closest("[data-close-flow]");
    const closeAbuseBtn = e.target.closest("[data-close-abuse]");
    const doneBtn = e.target.closest("#reportAnotherAnimal");

    if (openFlowBtn) {
        openFlow(openFlowBtn.dataset.openFlow);
        return;
    }

    if (nextBtn) {
        const nextStep = Number(nextBtn.dataset.nextStep);

        if (nextStep === 3 && !validateStep2()) return;

        showStep(nextStep);
        return;
    }

    if (prevBtn) {
        const prevStep = Number(prevBtn.dataset.prevStep);
        showStep(prevStep);
        return;
    }

    if (closeFlowBtn) {
        closeFlow();
        return;
    }

    if (closeAbuseBtn) {
        closeAbuseModal();
        return;
    }

    if (doneBtn) {
        closeFlow();
    }
});

if (submitAbuseBtn) {
    submitAbuseBtn.addEventListener("click", () => {
        const fraudDetails = document.getElementById("fraudDetails")?.value.trim() || "";

        if (!fraudDetails) {
            showToast("Please explain the issue before sending the report.");
            return;
        }

        showToast("Your abuse report has been submitted successfully.");
        closeAbuseModal();
    });
}

if (viewMoreBtn && petsGrid) {
    viewMoreBtn.addEventListener("click", () => {
        if (shownExtra) return;

        petsGrid.insertAdjacentHTML(
            "beforeend",
            extraPets.map((pet, index) => createPetCard(pet, `extra-${index + 1}`)).join("")
        );

        shownExtra = true;
        viewMoreBtn.textContent = "Done!!";
        viewMoreBtn.disabled = true;
        viewMoreBtn.style.opacity = "0.7";
    });
}

document.addEventListener("DOMContentLoaded", () => {
    updateCartBadge();
    injectProfileLostPet();
    initFadeUpAnimation();
    initServicesDropdown();
    initMobileMenu();
});