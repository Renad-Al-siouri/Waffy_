const cartCount = document.getElementById("cartCount");
const searchInput = document.getElementById("searchInput");
const tabButtons = document.querySelectorAll(".tab-btn");
const toast = document.getElementById("toast");

const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

let currentFilter = "all";
let mainMap;
let markersLayer = [];

const places = [
  {
    id: 1,
    type: "clinic",
    name: "Happy Paws Veterinary Hospital",
    address: "Sunnydale Park, Amman",
    phone: "+962 79 1234567",
    hours: "Mon - Sat: 9:00 AM - 7:00 PM",
    reviews: 48,
    stars: "★★★★★",
    distance: "2.4 km away",
    lat: 31.958,
    lng: 35.905,
    image: "../Images/gps/c1.png"
  },
  {
    id: 2,
    type: "clinic",
    name: "Green Paws Animal Clinic",
    address: "Waggin Plaza, Amman",
    phone: "+962 79 5551001",
    hours: "Sun - Thu: 10:00 AM - 8:00 PM",
    reviews: 31,
    stars: "★★★★☆",
    distance: "3.1 km away",
    lat: 31.949,
    lng: 35.899,
    image: "../Images/gps/c2.png"
  },
  {
    id: 3,
    type: "clinic",
    name: "PetCare Medical Center",
    address: "River Side, Amman",
    phone: "+962 79 7788991",
    hours: "Daily: 8:30 AM - 9:00 PM",
    reviews: 64,
    stars: "★★★★★",
    distance: "1.7 km away",
    lat: 31.953,
    lng: 35.918,
    image: "../Images/gps/c3.png"
  },
  {
    id: 7,
    type: "store",
    name: "Fluffy Supplies Store",
    address: "Park Lane, Amman",
    phone: "+962 79 1002020",
    hours: "Daily: 9:00 AM - 11:00 PM",
    reviews: 27,
    stars: "★★★★☆",
    distance: "1.1 km away",
    lat: 31.947,
    lng: 35.913,
    image: "../Images/gps/s1.png"
  },
  {
    id: 8,
    type: "store",
    name: "Happy Tail Pet Shop",
    address: "Downtown Amman",
    phone: "+962 79 3334400",
    hours: "Sat - Thu: 9:30 AM - 10:00 PM",
    reviews: 39,
    stars: "★★★★☆",
    distance: "4.0 km away",
    lat: 31.945,
    lng: 35.922,
    image: "../Images/gps/s2.png"
  },
  {
    id: 12,
    type: "playground",
    name: "Sunnydale Pet Playground",
    address: "Sunnydale Park",
    phone: "+962 79 2222000",
    hours: "Daily: 7:00 AM - 8:00 PM",
    reviews: 15,
    stars: "★★★★☆",
    distance: "2.8 km away",
    lat: 31.959,
    lng: 35.921,
    image: "../Images/gps/p1.png"
  },
  {
    id: 13,
    type: "playground",
    name: "Waggin Pet Play Yard",
    address: "Waggin Plaza",
    phone: "+962 79 4444555",
    hours: "Daily: 8:00 AM - 7:30 PM",
    reviews: 20,
    stars: "★★★★☆",
    distance: "3.4 km away",
    lat: 31.942,
    lng: 35.907,
    image: "../Images/gps/p2.png"
  }
];

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

function initFadeUpAnimation() {
  const fadeElements = document.querySelectorAll(".fade-up");

  if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add("show");
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.15 });

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

function filteredPlaces() {
  const q = searchInput.value.trim().toLowerCase();

  return places.filter((place) => {
    const matchFilter = currentFilter === "all" ? true : place.type === currentFilter;
    const matchSearch =
      place.name.toLowerCase().includes(q) ||
      place.address.toLowerCase().includes(q) ||
      place.type.toLowerCase().includes(q);

    return matchFilter && matchSearch;
  });
}

function popupActionsHTML(place) {
  const directionsLink = `https://www.google.com/maps/dir/?api=1&destination=${place.lat},${place.lng}`;

  let html = `
    <div class="map-popup-actions">
      <a class="popup-btn" href="${directionsLink}" target="_blank">🧭 Get Directions</a>
  `;

  if (place.type === "clinic") {
    html += `<a class="popup-btn secondary" href="veterinary.html">📅 View Schedule</a>`;
  }

  html += `</div>`;
  return html;
}

function popupHTML(place) {
  return `
    <div class="map-popup-card">
      <div class="map-popup-image">
        <img src="${place.image}" alt="${place.name}">
      </div>
      <div class="map-popup-body">
        <h3>${place.name}</h3>
        <p class="map-popup-type">${place.type.charAt(0).toUpperCase() + place.type.slice(1)}</p>
        <p class="map-popup-address">${place.address}</p>
        <p class="map-popup-stars">${place.stars} (${place.reviews})</p>
        <p class="map-popup-distance">${place.distance}</p>
        <p class="map-popup-phone">${place.phone}</p>
        <p class="map-popup-hours">${place.hours}</p>
        ${popupActionsHTML(place)}
      </div>
    </div>
  `;
}

function initMainMap() {
  mainMap = L.map("mainMap").setView([31.9539, 35.9106], 13);

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    attribution: "&copy; OpenStreetMap contributors"
  }).addTo(mainMap);

  renderMarkers();
}

function renderMarkers() {
  markersLayer.forEach((marker) => marker.remove());
  markersLayer = [];

  const visiblePlaces = filteredPlaces();

  visiblePlaces.forEach((place) => {
    const marker = L.marker([place.lat, place.lng]).addTo(mainMap);

    marker.bindPopup(popupHTML(place), {
      maxWidth: 320,
      className: "waffy-map-popup"
    });

    markersLayer.push(marker);
  });

  if (visiblePlaces.length) {
    const bounds = L.latLngBounds(visiblePlaces.map((p) => [p.lat, p.lng]));
    mainMap.fitBounds(bounds, { padding: [40, 40] });
  }
}

function initSearchAndTabs() {
  if (searchInput) {
    searchInput.addEventListener("input", renderMarkers);
  }

  tabButtons.forEach((btn) => {
    btn.addEventListener("click", () => {
      tabButtons.forEach((b) => b.classList.remove("active"));
      btn.classList.add("active");
      currentFilter = btn.dataset.filter;
      renderMarkers();
    });
  });
}

document.addEventListener("DOMContentLoaded", () => {
  updateCartBadge();
  initFadeUpAnimation();
  initServicesDropdown();
  initMobileMenu();
  initSearchAndTabs();
  initMainMap();
});