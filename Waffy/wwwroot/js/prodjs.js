const products = [
    {
        id: 1,
        name: "Premium Dog Food",
        category: "food",
        price: 19.99,
        weight: "5 kg",
        bought: "2.5k bought",
        description: "Nutritious dry food for adult dogs.",
        image: "~/Images/products/Premium.png"
    },
    {
        id: 2,
        name: "Premium Cat Food",
        category: "food",
        price: 15.99,
        weight: "4 kg",
        bought: "2.3k bought",
        description: "Balanced meal for healthy cats.",
        image: "~/Images/products/PremiumCat.png"
    },
    {
        id: 3,
        name: "Chicken Bites Treats",
        category: "treats",
        price: 8.99,
        weight: "250 g",
        bought: "2.3k bought",
        description: "Tasty reward bites for training.",
        image: "~/Images/products/Chicken.png"
    },
    {
        id: 4,
        name: "Chewy Bone Toy",
        category: "toys",
        price: 5.99,
        weight: null,
        bought: "2.3k bought",
        description: "Fun chewing toy for playful dogs.",
        image: "~/Images/products/Chewy.png"
    },
    {
        id: 5,
        name: "Dental Chews",
        category: "treats",
        price: 9.99,
        weight: "300 g",
        bought: "2.3k bought",
        description: "Chews that help clean teeth.",
        image: "~/Images/products/Dental.png"
    },
    {
        id: 6,
        name: "Squeaky Carrot Toy",
        category: "toys",
        price: 6.99,
        weight: null,
        bought: "2.4k bought",
        description: "Cute squeaky toy for pets.",
        image: "~/Images/products/Squeaky.png"
    },
    {
        id: 7,
        name: "Plush Duck Toy",
        category: "toys",
        price: 9.99,
        weight: null,
        bought: "2.3k bought",
        description: "Soft toy for cuddling and play.",
        image: "~/Images/products/Plush.png"
    },
    {
        id: 8,
        name: "Cat Scratching Post",
        category: "accessories",
        price: 34.99,
        weight: null,
        bought: "2.4k bought",
        description: "Scratching post for active cats.",
        image: "~/Images/products/Cat.png"
    },
    {
        id: 9,
        name: "Tick & Flea Shampoo",
        category: "care",
        price: 14.99,
        weight: "500 ml",
        bought: "2.2k bought",
        description: "Gentle shampoo for pet hygiene.",
        image: "~/Images/products/Tick.png"
    },
    {
        id: 10,
        name: "Deluxe Litter Box",
        category: "accessories",
        price: 24.99,
        weight: null,
        bought: "2.3k bought",
        description: "Comfortable litter box for cats.",
        image: "~/Images/products/Deluxe.png"
    },
    {
        id: 11,
        name: "Soft Pet Bed",
        category: "beds",
        price: 18.99,
        weight: null,
        bought: "1.8k bought",
        description: "Cozy sleeping bed for pets.",
        image: "~/Images/products/Soft.png"
    },
    {
        id: 12,
        name: "Adjustable Pet Leash",
        category: "accessories",
        price: 7.99,
        weight: null,
        bought: "1.5k bought",
        description: "Durable leash for daily walks.",
        image: "~/Images/products/Adjustable.png"
    },
    {
        id: 13,
        name: "Classic Pet Collar",
        category: "accessories",
        price: 5.99,
        weight: null,
        bought: "1.2k bought",
        description: "Simple and stylish collar.",
        image: "~/Images/products/Classic.png"
    },
    {
        id: 14,
        name: "Bird Seed Mix",
        category: "food",
        price: 4.99,
        weight: "1 kg",
        bought: "980 bought",
        description: "Healthy seed mix for birds.",
        image: "~/Images/products/Bird.png"
    },
    {
        id: 15,
        name: "Rabbit Food Pellets",
        category: "food",
        price: 6.99,
        weight: "2 kg",
        bought: "860 bought",
        description: "Tasty pellets for rabbits.",
        image: "~/Images/products/Rabbit.png"
    },
    {
        id: 16,
        name: "Frozen Snake Feed",
        category: "food",
        price: 10.99,
        weight: "Pack",
        bought: "620 bought",
        description: "Frozen feed for snakes.",
        image: "~/Images/products/Frozen.png"
    }
];

const productsGrid = document.getElementById("productsGrid");
const searchInput = document.getElementById("searchInput");
const priceFilter = document.getElementById("priceFilter");
const categoryButtons = document.querySelectorAll(".category-btn");
const cartCount = document.getElementById("cartCount");
const toast = document.getElementById("toast");

let currentCategory = "all";
const quantities = {};

function getCart() {
    return JSON.parse(localStorage.getItem("waffyCart") || "[]");
}

function saveCart(cart) {
    localStorage.setItem("waffyCart", JSON.stringify(cart));
}

function updateCartCount() {
    const cart = getCart();
    const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
    if (cartCount) cartCount.textContent = totalItems;
}

function showToast(message) {
    toast.textContent = message;
    toast.classList.add("show");

    clearTimeout(showToast.timer);
    showToast.timer = setTimeout(() => {
        toast.classList.remove("show");
    }, 2500);
}

function capitalize(text) {
    if (!text) return "";
    return text.charAt(0).toUpperCase() + text.slice(1);
}

function getFilteredProducts() {
    const searchValue = searchInput.value.trim().toLowerCase();
    const selectedPrice = priceFilter.value;

    return products.filter((product) => {
        const matchesCategory =
            currentCategory === "all" ? true : product.category === currentCategory;

        const matchesSearch =
            product.name.toLowerCase().includes(searchValue) ||
            product.description.toLowerCase().includes(searchValue) ||
            product.category.toLowerCase().includes(searchValue);

        const matchesPrice =
            selectedPrice === "all"
                ? true
                : Math.floor(product.price) === Number(selectedPrice);

        return matchesCategory && matchesSearch && matchesPrice;
    });
}

function renderProducts() {
    const filteredProducts = getFilteredProducts();

    if (!filteredProducts.length) {
        productsGrid.innerHTML = `
      <div class="no-products-card">
        <h3>No products found</h3>
        <p>Try changing the category, price, or search term.</p>
      </div>
    `;
        return;
    }

    productsGrid.innerHTML = filteredProducts.map((product) => {
        const qty = quantities[product.id] || 1;

        return `
      <article class="product-card">
        <div class="product-image-box">
          <img src="${product.image}" alt="${product.name}">
        </div>

        <div class="product-content">
          <h3 class="product-title">${product.name}</h3>
          <p class="product-price">${product.price.toFixed(2)} JD</p>
          <p class="product-bought">${product.bought}</p>
          <p class="product-category"><strong>Category:</strong> ${capitalize(product.category)}</p>
          ${product.weight ? `<p class="product-weight"><strong>Weight:</strong> ${product.weight}</p>` : ""}
          <p class="product-meta">${product.description}</p>

          <div class="product-bottom">
            <div class="qty-control">
              <button class="qty-btn" data-action="decrease" data-id="${product.id}">-</button>
              <span class="qty-value">${qty}</span>
              <button class="qty-btn" data-action="increase" data-id="${product.id}">+</button>
            </div>

            <button class="add-cart-btn" data-action="add-cart" data-id="${product.id}">
              🛒 Add to Cart
            </button>
          </div>
        </div>
      </article>
    `;
    }).join("");
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

document.addEventListener("click", (e) => {
    const categoryBtn = e.target.closest(".category-btn");
    const actionBtn = e.target.closest("[data-action]");

    if (categoryBtn) {
        categoryButtons.forEach((btn) => btn.classList.remove("active"));
        categoryBtn.classList.add("active");
        currentCategory = categoryBtn.dataset.category;
        renderProducts();
        return;
    }

    if (actionBtn) {
        const action = actionBtn.dataset.action;
        const id = Number(actionBtn.dataset.id);

        if (!quantities[id]) quantities[id] = 1;

        if (action === "increase") {
            quantities[id] += 1;
            renderProducts();
            return;
        }

        if (action === "decrease") {
            quantities[id] = Math.max(1, quantities[id] - 1);
            renderProducts();
            return;
        }

        if (action === "add-cart") {
            const product = products.find((item) => item.id === id);
            const quantity = quantities[id] || 1;
            const cart = getCart();

            const existing = cart.find((item) => item.id === id);
            if (existing) {
                existing.quantity += quantity;
            } else {
                cart.push({
                    id: product.id,
                    name: product.name,
                    price: product.price,
                    image: product.image,
                    category: product.category,
                    quantity
                });
            }

            saveCart(cart);
            updateCartCount();
            showToast(`${quantity} item(s) of "${product.name}" added to cart.`);
            quantities[id] = 1;
            renderProducts();
        }
    }
});

searchInput.addEventListener("input", renderProducts);
priceFilter.addEventListener("change", renderProducts);

document.addEventListener("DOMContentLoaded", () => {
    updateCartCount();
    renderProducts();
    initFadeUpAnimation();
    initDesktopServicesMenu();
    initMobileMenu();
});