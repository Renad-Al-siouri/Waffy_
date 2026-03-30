const pets = [
  {
    id: 1,
    type: "dog",
    name: "Max",
    breed: "Golden Retriever",
    age: "3 years",
    size: "Medium",
    temperament: "Friendly, Energetic",
    likes: 128,
    vaccinations: ["Rabies", "DHPP", "Kennel Cough"],
    health: "Healthy",
    gender: "Male",
    description: "A playful and affectionate dog who loves people and outdoor fun.",
    image: "../Images/adapt/golden.png"
  },
  {
    id: 2,
    type: "dog",
    name: "Bella",
    breed: "Beagle",
    age: "2 years",
    size: "Small",
    temperament: "Playful, Affectionate",
    likes: 94,
    vaccinations: ["Rabies", "DHPP"],
    health: "Mild allergy history",
    gender: "Female",
    description: "A sweet and lively companion who enjoys cuddles and short walks.",
    image: "../Images/adapt/Beagle.png"
  },
  {
    id: 3,
    type: "dog",
    name: "Rocky",
    breed: "German Shepherd",
    age: "4 years",
    size: "Large",
    temperament: "Loyal, Protective",
    likes: 141,
    vaccinations: ["Rabies", "DHPP", "Bordetella"],
    health: "Healthy",
    gender: "Male",
    description: "A smart, loyal dog with a calm nature and strong protective instincts.",
    image: "../Images/adapt/German Shepherd.png"
  },
  {
    id: 4,
    type: "dog",
    name: "Daisy",
    breed: "Shih Tzu",
    age: "1 year",
    size: "Small",
    temperament: "Sweet, Gentle",
    likes: 87,
    vaccinations: ["Rabies", "DHPP"],
    health: "Healthy",
    gender: "Female",
    description: "A gentle little dog who loves quiet spaces and soft attention.",
    image: "../Images/adapt/Shih.png"
  },
  {
    id: 5,
    type: "cat",
    name: "Luna",
    breed: "Persian Cat",
    age: "2 years",
    size: "Small",
    temperament: "Calm, Elegant",
    likes: 122,
    vaccinations: ["Rabies", "FVRCP"],
    health: "Healthy",
    gender: "Female",
    description: "A beautiful calm cat who loves soft beds and peaceful corners.",
    image: "../Images/adapt/PersianCat.png"
  },
  {
    id: 6,
    type: "cat",
    name: "Milo",
    breed: "Orange Tabby",
    age: "1.5 years",
    size: "Medium",
    temperament: "Curious, Friendly",
    likes: 110,
    vaccinations: ["Rabies", "FVRCP"],
    health: "Healthy",
    gender: "Male",
    description: "A curious cat who enjoys playtime, people, and sunny windows.",
    image: "../Images/adapt/OrangeTabby.png"
  },
  {
    id: 7,
    type: "rabbit",
    name: "Snowy",
    breed: "Mini Lop",
    age: "1 year",
    size: "Small",
    temperament: "Gentle, Quiet",
    likes: 76,
    vaccinations: ["Routine Check Complete"],
    health: "Healthy",
    gender: "Female",
    description: "A quiet little rabbit with a soft personality and a gentle heart.",
    image: "../Images/adapt/MiniLop.png"
  },
  {
    id: 8,
    type: "rabbit",
    name: "Coco",
    breed: "Dutch Rabbit",
    age: "8 months",
    size: "Small",
    temperament: "Playful, Alert",
    likes: 68,
    vaccinations: ["Routine Check Complete"],
    health: "Healthy",
    gender: "Male",
    description: "An alert and playful rabbit who enjoys hopping and exploring.",
    image: "../Images/adapt/DutchRabbit.png"
  },
  {
    id: 9,
    type: "bird",
    name: "Sunny",
    breed: "Budgie",
    age: "10 months",
    size: "Small",
    temperament: "Cheerful, Social",
    likes: 83,
    vaccinations: ["Health Check Complete"],
    health: "Healthy",
    gender: "Female",
    description: "A cheerful bird with a bright personality and sweet chirps.",
    image: "../Images/adapt/Budgie.png"
  },
  {
    id: 10,
    type: "bird",
    name: "Kiwi",
    breed: "Lovebird",
    age: "1 year",
    size: "Small",
    temperament: "Active, Affectionate",
    likes: 72,
    vaccinations: ["Health Check Complete"],
    health: "Healthy",
    gender: "Male",
    description: "An active and affectionate bird who enjoys interaction and toys.",
    image: "../Images/adapt/Lovebird.png"
  },
  {
    id: 11,
    type: "snake",
    name: "Nova",
    breed: "Ball Python",
    age: "2 years",
    size: "Medium",
    temperament: "Calm, Curious",
    likes: 61,
    vaccinations: ["Vet Exam Complete"],
    health: "Healthy",
    gender: "Female",
    description: "A calm snake with a curious nature and beautiful patterns.",
    image: "../Images/adapt/BallPython.png"
  },
  {
    id: 12,
    type: "snake",
    name: "Sandy",
    breed: "Corn Snake",
    age: "1.5 years",
    size: "Medium",
    temperament: "Docile, Active",
    likes: 58,
    vaccinations: ["Vet Exam Complete"],
    health: "Healthy",
    gender: "Male",
    description: "A docile snake that is easy to handle and full of movement.",
    image: "../Images/adapt/CornSnake.png"
  },
  {
    id: 13,
    type: "hamster",
    name: "Nibbles",
    breed: "Syrian Hamster",
    age: "7 months",
    size: "Small",
    temperament: "Cute, Shy",
    likes: 66,
    vaccinations: ["Routine Check Complete"],
    health: "Healthy",
    gender: "Female",
    description: "A shy and adorable hamster who loves cozy spaces and seeds.",
    image: "../Images/adapt/SyrianHamster.png"
  },
  {
    id: 14,
    type: "hamster",
    name: "Peanut",
    breed: "Dwarf Hamster",
    age: "6 months",
    size: "Tiny",
    temperament: "Fast, Playful",
    likes: 71,
    vaccinations: ["Routine Check Complete"],
    health: "Healthy",
    gender: "Male",
    description: "A tiny playful hamster with lots of energy and charm.",
    image: "../Images/adapt/Dwarf.png"
  }
];

const petsGrid = document.getElementById("petsGrid");
const filterButtons = document.querySelectorAll(".filter-item");
const modal = document.getElementById("detailsModal");
const modalContent = document.getElementById("modalContent");
const toast = document.getElementById("toast");
const searchInput = document.getElementById("petSearch");

let currentFilter = "all";
let currentSearch = "";

/* ===== CART BADGE ===== */
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

function temperamentBars(text) {
  let count = 3;
  if (text.includes("Energetic") || text.includes("Protective") || text.includes("Active")) count = 4;
  if (text.includes("Gentle") || text.includes("Calm") || text.includes("Quiet")) count = 3;

  return `
    <div class="temper-boxes">
      ${Array.from({ length: 5 }, (_, i) => `<span class="${i < count ? "on" : ""}"></span>`).join("")}
    </div>
  `;
}

function getFilteredPets() {
  let filteredPets = currentFilter === "all"
    ? [...pets]
    : pets.filter((pet) => pet.type === currentFilter);

  if (currentSearch.trim()) {
    const query = currentSearch.trim().toLowerCase();
    filteredPets = filteredPets.filter((pet) =>
      pet.name.toLowerCase().includes(query) ||
      pet.breed.toLowerCase().includes(query) ||
      pet.size.toLowerCase().includes(query) ||
      pet.temperament.toLowerCase().includes(query)
    );
  }

  return filteredPets;
}

function renderPets() {
  const filteredPets = getFilteredPets();

  if (!filteredPets.length) {
    petsGrid.innerHTML = `
      <div class="empty-state">
        <h3>No pets found</h3>
        <p>Try another filter or search keyword.</p>
      </div>
    `;
    return;
  }

  petsGrid.innerHTML = filteredPets.map((pet) => `
    <article class="pet-item">
      <div class="pet-thumb">
        <img src="${pet.image}" alt="${pet.name}">
      </div>

      <div class="pet-info">
        <h3>${pet.name}</h3>
        <p><strong>Breed:</strong> ${pet.breed}</p>
        <p><strong>Age:</strong> ${pet.age}</p>
        <p><strong>Size:</strong> ${pet.size}</p>
        <p><strong>Temperament:</strong> ${pet.temperament}</p>
        ${temperamentBars(pet.temperament)}
      </div>

      <div class="pet-actions">
        <div class="like-wrap">
          <button class="like-btn" data-like="${pet.id}">❤</button>
          <span class="likes-count"><span id="likes-${pet.id}">${pet.likes}</span> likes</span>
        </div>

        <button class="olive-btn" data-reserve="${pet.id}">Reserve</button>
        <button class="olive-btn secondary-btn" data-details="${pet.id}">View Details</button>
      </div>
    </article>
  `).join("");
}

function openDetails(petId) {
  const pet = pets.find((item) => item.id === Number(petId));
  if (!pet) return;

  modalContent.innerHTML = `
    <div class="modal-layout">
      <div class="modal-image-box">
        <img src="${pet.image}" alt="${pet.name}">
      </div>

      <div class="modal-info">
        <h3>${pet.name}</h3>
        <p><strong>Breed:</strong> ${pet.breed}</p>
        <p><strong>Age:</strong> ${pet.age}</p>
        <p><strong>Size:</strong> ${pet.size}</p>
        <p><strong>Gender:</strong> ${pet.gender}</p>
        <p><strong>Temperament:</strong> ${pet.temperament}</p>
        <p><strong>Health Status:</strong> ${pet.health}</p>
        <p><strong>Description:</strong> ${pet.description}</p>

        <div class="modal-grid">
          <div class="modal-box">
            <h4>Vaccinations</h4>
            <ul>
              ${pet.vaccinations.map((item) => `<li>${item}</li>`).join("")}
            </ul>
          </div>

          <div class="modal-box">
            <h4>Adoption Notes</h4>
            <ul>
              <li>Friendly with gentle handling</li>
              <li>Needs a loving home</li>
              <li>Vet-checked before adoption</li>
              <li>Ready for reservation</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  `;

  modal.classList.remove("hidden");
}

function closeDetails() {
  modal.classList.add("hidden");
}

function showToast(message) {
  toast.textContent = message;
  toast.classList.add("show");

  clearTimeout(showToast.timer);
  showToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 3200);
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

filterButtons.forEach((btn) => {
  btn.addEventListener("click", () => {
    filterButtons.forEach((item) => item.classList.remove("active"));
    btn.classList.add("active");
    currentFilter = btn.dataset.filter;
    renderPets();
  });
});

if (searchInput) {
  searchInput.addEventListener("input", (e) => {
    currentSearch = e.target.value;
    renderPets();
  });
}

document.addEventListener("click", (e) => {
  const likeBtn = e.target.closest("[data-like]");
  const reserveBtn = e.target.closest("[data-reserve]");
  const detailsBtn = e.target.closest("[data-details]");
  const closeBtn = e.target.closest("[data-close-modal]");

  if (likeBtn) {
    const petId = Number(likeBtn.dataset.like);
    const pet = pets.find((item) => item.id === petId);
    if (!pet) return;

    pet.likes += 1;
    const counter = document.getElementById(`likes-${petId}`);
    if (counter) counter.textContent = pet.likes;
    likeBtn.classList.add("active");
    return;
  }

  if (reserveBtn) {
    const petId = Number(reserveBtn.dataset.reserve);
    const pet = pets.find((item) => item.id === petId);
    if (!pet) return;

    showToast(`Thank you for reserving ${pet.name}! We’re so happy and grateful for your kindness. Your reservation has been received successfully.`);
    return;
  }

  if (detailsBtn) {
    openDetails(detailsBtn.dataset.details);
    return;
  }

  if (closeBtn) {
    closeDetails();
  }
});

document.addEventListener("keydown", (e) => {
  if (e.key === "Escape") closeDetails();
});

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  renderPets();
  initFadeUpAnimation();
  initDesktopServicesMenu();
  initMobileMenu();
});