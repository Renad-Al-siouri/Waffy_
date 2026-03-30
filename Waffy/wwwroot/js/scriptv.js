const clinics = [
  {
    id: 1,
    name: "Happy Paws Veterinary Hospital",
    shortName: "Happy Paws",
    reviews: 215,
    rating: 5,
    description: "Full-service clinic offering surgeries, dental care, and emergency services.",
    details: "Happy Paws offers advanced surgery, emergency support, dental procedures, diagnostic services, and post-treatment care in a friendly pet-focused environment.",
    address: "Amman - Khalda",
    phone: "+962 79 512 3456",
    hours: "Mon – Fri: 9:00 AM - 8:00 PM",
    workingDays: [
      "Mon – Fri: 9:00 AM - 8:00 PM",
      "Sat: 9:00 AM - 5:00 PM",
      "Sun: Closed"
    ],
    services: [
      { name: "Vaccination", price: "12 JD" },
      { name: "Surgery", price: "80 JD" },
      { name: "Dental Care", price: "35 JD" },
      { name: "Emergency Care", price: "45 JD" }
    ],
    badge: "🐾",
    cardImage: "../Images/clinics/cimage1.png",
    detailImage: "../Images/clinics/cimage1.png",
    mapLat: 31.958,
    mapLng: 35.905,
    doctors: [
      { name: "Dr. Sarah Khalil", specialty: "Surgeon", phone: "+962 79 512 3456", image: "../Images/doctors/sara.png" },
      { name: "Dr. Omar Haddad", specialty: "Dermatology", phone: "+962 79 512 3456", image: "../Images/doctors/omar.png" },
      { name: "Dr. Lina Youssef", specialty: "General Vet", phone: "+962 79 512 3456", image: "../Images/doctors/lina.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["11:00 AM", "2:00 PM", "5:00 PM", "7:30 PM"] },
      { day: "Tomorrow, April 26", slots: ["9:00 AM", "11:30 AM", "3:00 PM"] },
      { day: "Saturday, April 27", slots: ["10:00 AM", "1:30 PM"] }
    ]
  },
  {
    id: 2,
    name: "Green Leaf Vet Clinic",
    shortName: "Green Leaf",
    reviews: 180,
    rating: 5,
    description: "Holistic treatments, wellness exams, and preventive care tailored to pets.",
    details: "Green Leaf focuses on preventive medicine, nutrition planning, wellness exams, and routine follow-up programs to support long-term pet health.",
    address: "Amman - Dabouq",
    phone: "+962 79 411 2233",
    hours: "Sat – Thu: 10:00 AM - 8:00 PM",
    workingDays: [
      "Sat – Thu: 10:00 AM - 8:00 PM",
      "Fri: Closed"
    ],
    services: [
      { name: "Wellness Exams", price: "15 JD" },
      { name: "Vaccinations", price: "10 JD" },
      { name: "Nutrition Advice", price: "18 JD" },
      { name: "Preventive Care", price: "20 JD" }
    ],
    badge: "🐾",
    cardImage: "../Images/clinics/cimage3.png",
    detailImage: "../Images/clinics/cimage3.png",
    mapLat: 31.986,
    mapLng: 35.835,
    doctors: [
      { name: "Dr. Maya Salem", specialty: "Nutrition Vet", phone: "+962 79 411 2233", image: "../Images/doctors/maya.png" },
      { name: "Dr. Nabil Saad", specialty: "Preventive Care", phone: "+962 79 411 2233", image: "../Images/doctors/nabil.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["12:00 PM", "4:00 PM", "6:30 PM"] },
      { day: "Tomorrow, April 26", slots: ["10:00 AM", "1:00 PM", "5:00 PM"] },
      { day: "Sunday, April 28", slots: ["9:30 AM", "2:30 PM", "4:30 PM"] }
    ]
  },
  {
    id: 3,
    name: "PetCare Animal Center",
    shortName: "PetCare",
    reviews: 342,
    rating: 5,
    description: "Advanced diagnostics, dermatology care, and surgical procedures.",
    details: "PetCare Animal Center is equipped for complex diagnostics, dermatology support, referrals, lab testing, and specialized treatment plans.",
    address: "Amman - Medical Avenue",
    phone: "+962 79 622 7788",
    hours: "Sat – Thu: 8:30 AM - 7:30 PM",
    workingDays: [
      "Sat – Thu: 8:30 AM - 7:30 PM",
      "Fri: Closed"
    ],
    services: [
      { name: "Lab Testing", price: "22 JD" },
      { name: "Dermatology", price: "28 JD" },
      { name: "Advanced Diagnostics", price: "40 JD" },
      { name: "Specialized Procedures", price: "65 JD" }
    ],
    badge: "✚",
    cardImage: "../Images/clinics/cimage2.png",
    detailImage: "../Images/clinics/cimage2.png",
    mapLat: 31.953,
    mapLng: 35.918,
    doctors: [
      { name: "Dr. Rana Fares", specialty: "Dermatology", phone: "+962 79 622 7788", image: "../Images/doctors/rana.png" },
      { name: "Dr. Yousef Adel", specialty: "Diagnostics", phone: "+962 79 622 7788", image: "../Images/doctors/yousef.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["1:00 PM", "3:30 PM", "6:00 PM"] },
      { day: "Tomorrow, April 26", slots: ["9:30 AM", "12:30 PM"] },
      { day: "Monday, April 29", slots: ["10:30 AM", "2:00 PM", "5:30 PM"] }
    ]
  },
  {
    id: 4,
    name: "Happy Tails Animal Clinic",
    shortName: "Happy Tails",
    reviews: 278,
    rating: 5,
    description: "Routine checkups, vaccinations, and dental cleaning with follow-up care.",
    details: "Happy Tails is ideal for routine visits, annual vaccines, quick consultations, dental cleaning, and recurring care for family pets.",
    address: "Amman - Sweifieh",
    phone: "+962 79 733 5566",
    hours: "Sun – Thu: 9:00 AM - 6:00 PM",
    workingDays: [
      "Sun – Thu: 9:00 AM - 6:00 PM",
      "Fri – Sat: Closed"
    ],
    services: [
      { name: "Vaccinations", price: "10 JD" },
      { name: "Dental Cleaning", price: "32 JD" },
      { name: "Routine Checkups", price: "14 JD" },
      { name: "Quick Consultations", price: "12 JD" }
    ],
    badge: "🐕",
    cardImage: "../Images/clinics/cimage4.png",
    detailImage: "../Images/clinics/cimage4.png",
    mapLat: 31.958,
    mapLng: 35.859,
    doctors: [
      { name: "Dr. Farah Nasser", specialty: "General Vet", phone: "+962 79 733 5566", image: "../Images/doctors/farah.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["10:30 AM", "1:30 PM", "4:30 PM"] },
      { day: "Tomorrow, April 26", slots: ["11:00 AM", "2:00 PM", "5:00 PM"] },
      { day: "Tuesday, April 30", slots: ["9:00 AM", "12:00 PM", "3:00 PM"] }
    ]
  },
  {
    id: 5,
    name: "Green Paws Animal Clinic",
    shortName: "Green Paws",
    reviews: 31,
    rating: 4,
    description: "Friendly clinic for wellness checks, vaccinations, and general care.",
    details: "Green Paws Animal Clinic provides daily consultations, vaccination plans, nutrition support, and follow-up treatment for common pet health needs.",
    address: "Waggin Plaza, Amman",
    phone: "+962 79 555 1001",
    hours: "Sun – Thu: 10:00 AM - 8:00 PM",
    workingDays: [
      "Sun – Thu: 10:00 AM - 8:00 PM",
      "Fri: Closed",
      "Sat: 10:00 AM - 4:00 PM"
    ],
    services: [
      { name: "General Checkup", price: "15 JD" },
      { name: "Vaccinations", price: "11 JD" },
      { name: "Nutrition Plan", price: "18 JD" },
      { name: "Follow-up Visit", price: "14 JD" }
    ],
    badge: "🐾",
    cardImage: "../Images/gps/c2.png",
    detailImage: "../Images/gps/c2.png",
    mapLat: 31.949,
    mapLng: 35.899,
    doctors: [
      { name: "Dr. Hadeel Sami", specialty: "General Vet", phone: "+962 79 555 1001", image: "../Images/doctors/hadeel.png" },
      { name: "Dr. Karim Zaid", specialty: "Preventive Care", phone: "+962 79 555 1001", image: "../Images/doctors/karem.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["10:00 AM", "1:00 PM", "4:00 PM"] },
      { day: "Tomorrow, April 26", slots: ["11:00 AM", "2:30 PM", "5:00 PM"] },
      { day: "Sunday, April 28", slots: ["9:30 AM", "12:30 PM"] }
    ]
  },
  {
    id: 6,
    name: "PetCare Medical Center",
    shortName: "Medical Center",
    reviews: 64,
    rating: 5,
    description: "Advanced veterinary services, diagnostics, and specialty procedures.",
    details: "PetCare Medical Center is a larger veterinary facility that supports referrals, diagnostics, surgery preparation, and specialized treatment planning.",
    address: "River Side, Amman",
    phone: "+962 79 778 8991",
    hours: "Daily: 8:30 AM - 9:00 PM",
    workingDays: [
      "Mon – Sun: 8:30 AM - 9:00 PM"
    ],
    services: [
      { name: "Diagnostic Scan", price: "35 JD" },
      { name: "Dermatology Visit", price: "25 JD" },
      { name: "Surgical Evaluation", price: "30 JD" },
      { name: "Emergency Support", price: "50 JD" }
    ],
    badge: "✚",
    cardImage: "../Images/gps/c3.png",
    detailImage: "../Images/gps/c3.png",
    mapLat: 31.953,
    mapLng: 35.918,
    doctors: [
      { name: "Dr. Layla Tamer", specialty: "Diagnostics", phone: "+962 79 778 8991", image: "../Images/doctors/laila.png" },
      { name: "Dr. Sami Hani", specialty: "Emergency Care", phone: "+962 79 778 8991", image: "../Images/doctors/sami.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["9:00 AM", "12:00 PM", "5:00 PM"] },
      { day: "Tomorrow, April 26", slots: ["10:30 AM", "2:30 PM", "7:00 PM"] },
      { day: "Monday, April 29", slots: ["11:00 AM", "1:00 PM", "4:30 PM"] }
    ]
  },
  {
    id: 7,
    name: "Royal Vet House",
    shortName: "Royal Vet",
    reviews: 41,
    rating: 4,
    description: "Comfortable clinic for family pets with consultations and treatments.",
    details: "Royal Vet House offers treatment plans for cats and dogs, regular monitoring visits, lab referrals, and preventive medicine.",
    address: "Abdoun, Amman",
    phone: "+962 79 300 4455",
    hours: "Daily: 9:00 AM - 8:30 PM",
    workingDays: [
      "Mon – Sun: 9:00 AM - 8:30 PM"
    ],
    services: [
      { name: "Consultation", price: "16 JD" },
      { name: "Lab Referral", price: "20 JD" },
      { name: "Vaccination", price: "11 JD" },
      { name: "Minor Procedure", price: "38 JD" }
    ],
    badge: "🏥",
    cardImage: "../Images/gps/c4.png",
    detailImage: "../Images/gps/c4.png",
    mapLat: 31.941,
    mapLng: 35.887,
    doctors: [
      { name: "Dr. Nour Khoury", specialty: "General Vet", phone: "+962 79 300 4455", image: "../Images/doctors/noor.png" },
      { name: "Dr. Adam Saleh", specialty: "Internal Medicine", phone: "+962 79 300 4455", image: "../Images/doctors/adam.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["10:00 AM", "12:30 PM", "6:00 PM"] },
      { day: "Tomorrow, April 26", slots: ["9:30 AM", "1:30 PM", "4:30 PM"] },
      { day: "Tuesday, April 30", slots: ["11:00 AM", "3:00 PM"] }
    ]
  },
  {
    id: 8,
    name: "Care & Cure Pet Clinic",
    shortName: "Care & Cure",
    reviews: 29,
    rating: 4,
    description: "Reliable clinic with vaccination, checkups, and pet treatment follow-up.",
    details: "Care & Cure Pet Clinic supports regular health checks, treatment follow-up, early diagnosis, and routine procedures for household pets.",
    address: "Khalda, Amman",
    phone: "+962 79 667 7882",
    hours: "Sat – Thu: 10:00 AM - 9:00 PM",
    workingDays: [
      "Sat – Thu: 10:00 AM - 9:00 PM",
      "Fri: Closed"
    ],
    services: [
      { name: "Health Check", price: "14 JD" },
      { name: "Vaccination", price: "10 JD" },
      { name: "Treatment Follow-up", price: "16 JD" },
      { name: "Minor Emergency", price: "34 JD" }
    ],
    badge: "💚",
    cardImage: "../Images/gps/c5.png",
    detailImage: "../Images/gps/c5.png",
    mapLat: 31.995,
    mapLng: 35.861,
    doctors: [
      { name: "Dr. Salma Odeh", specialty: "General Practice", phone: "+962 79 667 7882", image: "../Images/doctors/salama.png" },
      { name: "Dr. Ibrahim Naser", specialty: "Preventive Care", phone: "+962 79 667 7882", image: "../Images/doctors/ibrahim.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["10:30 AM", "2:00 PM", "6:30 PM"] },
      { day: "Tomorrow, April 26", slots: ["11:30 AM", "3:30 PM", "7:00 PM"] },
      { day: "Sunday, April 28", slots: ["12:00 PM", "5:00 PM"] }
    ]
  },
  {
    id: 9,
    name: "Little Tails Vet Point",
    shortName: "Little Tails",
    reviews: 18,
    rating: 4,
    description: "Small and friendly neighborhood clinic for routine pet needs.",
    details: "Little Tails Vet Point is ideal for neighborhood pet owners looking for quick consultations, vaccines, light procedures, and regular checkups.",
    address: "Dabouq, Amman",
    phone: "+962 79 432 1110",
    hours: "Daily: 9:30 AM - 6:30 PM",
    workingDays: [
      "Mon – Sun: 9:30 AM - 6:30 PM"
    ],
    services: [
      { name: "Quick Consultation", price: "12 JD" },
      { name: "Routine Checkup", price: "13 JD" },
      { name: "Vaccination", price: "10 JD" },
      { name: "Skin Check", price: "18 JD" }
    ],
    badge: "🐶",
    cardImage: "../Images/gps/c6.png",
    detailImage: "../Images/gps/c6.png",
    mapLat: 31.991,
    mapLng: 35.832,
    doctors: [
      { name: "Dr. Dana Rami", specialty: "Routine Care", phone: "+962 79 432 1110", image: "../Images/doctors/dana.png" }
    ],
    schedule: [
      { day: "Today, April 25", slots: ["9:30 AM", "12:00 PM", "4:00 PM"] },
      { day: "Tomorrow, April 26", slots: ["10:30 AM", "1:30 PM", "5:30 PM"] },
      { day: "Monday, April 29", slots: ["11:30 AM", "3:30 PM"] }
    ]
  }
];

const clinicPlacesData = clinics.map((clinic) => ({
  id: clinic.id,
  type: "clinic",
  name: clinic.name,
  address: clinic.address,
  phone: clinic.phone,
  hours: clinic.hours,
  reviews: clinic.reviews,
  stars: "★".repeat(clinic.rating),
  distance: "",
  lat: clinic.mapLat,
  lng: clinic.mapLng,
  image: clinic.cardImage
}));

localStorage.setItem("clinicPlacesData", JSON.stringify(clinicPlacesData));

const state = {
  expandedCards: new Set([1]),
  selectedSlots: {},
  selectedServices: {}
};

const cardsContainer = document.getElementById("cardsContainer");
const searchInput = document.getElementById("searchInput");
const collapseAllBtn = document.getElementById("collapseAllBtn");
const listPage = document.getElementById("listPage");
const detailPage = document.getElementById("detailPage");
const detailCard = document.getElementById("detailCard");
const backBtn = document.getElementById("backBtn");
const toast = document.getElementById("toast");
const cartCount = document.getElementById("cartCount");

function getCart() {
  return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function updateCartBadge() {
  const cart = getCart();
  const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 0), 0);
  if (cartCount) cartCount.textContent = totalItems;
}

function renderStars(count) {
  return "★".repeat(count);
}

function renderCards(items) {
  cardsContainer.innerHTML = items.map((clinic) => {
    const expanded = state.expandedCards.has(clinic.id);
    const selected = state.selectedSlots[clinic.id];
    const selectedService = state.selectedServices[clinic.id] || "";

    return `
      <article class="clinic-card ${expanded ? "expanded" : ""}" data-id="${clinic.id}">
        <div class="card-main" data-open-detail="${clinic.id}">
          <div class="card-left">
            <div class="card-head">
              <button class="schedule-btn" type="button" data-toggle-schedule="${clinic.id}">
                ${expanded ? "Hide Schedule" : "View Schedule"}
              </button>

              <div style="min-width:0; flex:1;">
                <h3 class="clinic-title">${clinic.name}</h3>
                <p class="clinic-desc">${clinic.description}</p>

                <div class="meta-row">
                  <span class="stars">${renderStars(clinic.rating)}</span>
                  <span>(${clinic.reviews} reviews)</span>
                  <span>•</span>
                  <span>${clinic.hours}</span>
                </div>
              </div>
            </div>
          </div>

          <div class="clinic-visual">
            <img src="${clinic.cardImage}" alt="${clinic.name}" class="clinic-card-image" />
            <div class="clinic-badge">${clinic.badge}</div>
          </div>
        </div>

        <div class="schedule-panel">
          <div class="schedule-wrap">
            <div class="service-row">
              <div class="service-label">Choose Service</div>
              <select class="service-select" data-service-select="${clinic.id}">
                <option value="">Select a service first</option>
                ${clinic.services.map(service => `
                  <option value="${service.name}" ${selectedService === service.name ? "selected" : ""}>
                    ${service.name} — ${service.price}
                  </option>
                `).join("")}
              </select>
            </div>

            <div class="days">
              ${clinic.schedule.map((entry, index) => `
                <div class="day-row">
                  <div class="day-label">• ${entry.day}</div>

                  <div class="slots">
                    ${entry.slots.map((slot) => {
                      const slotKey = `${entry.day}__${slot}`;
                      const active = selected === slotKey;
                      return `
                        <button
                          class="slot ${active ? "selected" : ""}"
                          type="button"
                          data-slot
                          data-clinic-id="${clinic.id}"
                          data-day="${entry.day}"
                          data-time="${slot}"
                        >${slot}</button>
                      `;
                    }).join("")}
                  </div>

                  <div>
                    ${index === clinic.schedule.length - 1
                      ? `<button class="confirm-btn" type="button" data-confirm="${clinic.id}">Confirm Appointment</button>`
                      : ""
                    }
                  </div>
                </div>
              `).join("")}
            </div>
          </div>
        </div>
      </article>
    `;
  }).join("");
}

function showClinicDetail(clinicId) {
  const clinic = clinics.find(item => item.id === Number(clinicId));
  if (!clinic) return;

  detailCard.innerHTML = `
    <h2 class="detail-page-title">${clinic.name}</h2>

    <div class="clinic-detail-top">
      <div class="clinic-detail-image-wrap">
        <img src="${clinic.detailImage}" alt="${clinic.name}" class="clinic-detail-image" />
      </div>

      <div class="clinic-detail-info">
        <h3>${clinic.name}</h3>

        <div class="detail-line">📍 <span>${clinic.address}</span></div>
        <div class="detail-line">
          <span class="stars">${renderStars(clinic.rating)}</span>
          <span>(${clinic.reviews} reviews)</span>
        </div>
        <div class="detail-line">📞 <span>${clinic.phone}</span></div>

        <div class="detail-separator"></div>

        ${clinic.workingDays.map(day => `
          <div class="detail-line">🐾 <span>${day}</span></div>
        `).join("")}
      </div>
    </div>

    <div class="clinic-detail-grid">
      <div class="doctors-section">
        <h4>Our Veterinarians</h4>

        <div class="doctors-list">
          ${clinic.doctors.map(doctor => `
            <div class="doctor-card">
              <div class="doctor-image-wrap">
                <img src="${doctor.image}" alt="${doctor.name}" class="doctor-image" />
              </div>

              <div class="doctor-info">
                <h5>${doctor.name}</h5>
                <div class="doctor-meta">🩺 <span>${doctor.specialty}</span></div>
                <div class="doctor-meta">📞 <span>${doctor.phone}</span></div>
              </div>
            </div>
          `).join("")}
        </div>
      </div>

      <div class="services-section">
        <div class="services-box">
          <h4>Services:</h4>
          <ul class="services-list">
            ${clinic.services.map(service => `
              <li>
                <span class="service-name-wrap">${service.name}</span>
                <span class="service-price">${service.price}</span>
              </li>
            `).join("")}
          </ul>
        </div>
      </div>
    </div>
  `;

  listPage.classList.add("hidden");
  detailPage.classList.remove("hidden");
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function showListPage() {
  detailPage.classList.add("hidden");
  listPage.classList.remove("hidden");
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function showToast(message) {
  toast.textContent = message;
  toast.classList.add("show");
  clearTimeout(showToast.timer);
  showToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 2800);
}

function getFilteredClinics() {
  const query = searchInput.value.trim().toLowerCase();
  if (!query) return clinics;

  return clinics.filter((clinic) =>
    clinic.name.toLowerCase().includes(query) ||
    clinic.shortName.toLowerCase().includes(query) ||
    clinic.description.toLowerCase().includes(query) ||
    clinic.services.map(service => service.name).join(" ").toLowerCase().includes(query)
  );
}

function refresh() {
  renderCards(getFilteredClinics());
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

cardsContainer.addEventListener("click", (event) => {
  const toggleBtn = event.target.closest("[data-toggle-schedule]");
  const slotBtn = event.target.closest("[data-slot]");
  const confirmBtn = event.target.closest("[data-confirm]");
  const detailArea = event.target.closest("[data-open-detail]");

  if (toggleBtn) {
    event.stopPropagation();
    const id = Number(toggleBtn.dataset.toggleSchedule);

    if (state.expandedCards.has(id)) {
      state.expandedCards.delete(id);
    } else {
      state.expandedCards.add(id);
    }

    refresh();
    return;
  }

  if (slotBtn) {
    event.stopPropagation();
    const clinicId = Number(slotBtn.dataset.clinicId);
    const day = slotBtn.dataset.day;
    const time = slotBtn.dataset.time;
    state.selectedSlots[clinicId] = `${day}__${time}`;
    refresh();
    return;
  }

  if (confirmBtn) {
    event.stopPropagation();
    const clinicId = Number(confirmBtn.dataset.confirm);
    const clinic = clinics.find(item => item.id === clinicId);
    const selectedSlot = state.selectedSlots[clinicId];
    const selectedService = state.selectedServices[clinicId];

    if (!selectedService) {
      showToast("Please select a service first.");
      return;
    }

    if (!selectedSlot) {
      showToast("Please select a time slot first.");
      return;
    }

    const [day, time] = selectedSlot.split("__");
    showToast(`Your ${selectedService} appointment at ${clinic.shortName} has been confirmed for ${day} at ${time}.`);
    return;
  }

  if (detailArea) {
    const id = Number(detailArea.dataset.openDetail);
    showClinicDetail(id);
  }
});

cardsContainer.addEventListener("change", (event) => {
  const serviceSelect = event.target.closest("[data-service-select]");
  if (!serviceSelect) return;

  const clinicId = Number(serviceSelect.dataset.serviceSelect);
  state.selectedServices[clinicId] = serviceSelect.value;
});

searchInput.addEventListener("input", refresh);

collapseAllBtn.addEventListener("click", () => {
  state.expandedCards.clear();
  refresh();
});

backBtn.addEventListener("click", showListPage);

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  refresh();
  initDesktopServicesMenu();
  initMobileMenu();
  initFadeUpAnimation();
});