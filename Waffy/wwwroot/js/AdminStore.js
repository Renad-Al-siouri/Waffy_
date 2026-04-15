let adoptionViewMode = "cards";
let adoptionMode = "clinic";
let productImageData = "";
let adoptionImageData = "";
let editingProductIndex = null;

const categories = [
    { name: "Food", icon: "🍖", count: 18 },
    { name: "Accessories", icon: "🎀", count: 12 },
    { name: "Health Care", icon: "💊", count: 9 },
    { name: "Toys", icon: "🧸", count: 9 }
];

let products = window.serverProducts || [];

let orders = window.serverOrders || [];

let customers = window.serverCustomers || [];

let reviews = [
    { user: "Sarah M.", prod: "Royal Canin Cat 2kg", rating: 5, text: "Excellent product quality and fast delivery. Thank you!", replied: false, time: "1 hour ago" },
    { user: "Ahmad A.", prod: "GPS Dog Collar", rating: 4, text: "Very accurate collar, but the battery could last longer.", replied: true, time: "2 days ago" },
    { user: "Lama H.", prod: "Anti-Flea Shampoo", rating: 3, text: "Good product, but the quantity feels small for the price.", replied: false, time: "3 days ago" },
    { user: "Reem A.", prod: "Cat Ball Toy", rating: 5, text: "My cat loves it and keeps playing all day.", replied: true, time: "1 week ago" }
];

let adoptionPets = [
    {
        id: 1,
        name: "Mishmish",
        type: "cat",
        typeLabel: "Cat",
        breed: "Persian Mix",
        age: "2 years",
        gender: "Female",
        color: "Cream",
        health: "Healthy",
        vacc: "Yes",
        neuter: "Yes",
        vaccDate: "2026-03-01",
        problems: "",
        traits: "Friendly, calm, loving",
        energy: "Low",
        children: "Yes",
        otherPets: "Yes",
        adStatus: "approved",
        adoptStatus: "available",
        desc: "A rescued cat with a calm and affectionate personality.",
        image: ""
    },
    {
        id: 2,
        name: "Rocky",
        type: "dog",
        typeLabel: "Dog",
        breed: "Mixed",
        age: "1 year",
        gender: "Male",
        color: "Brown",
        health: "Has health issues",
        vacc: "Yes",
        neuter: "Yes",
        vaccDate: "2026-02-14",
        problems: "Needs special care",
        traits: "Loyal, calm",
        energy: "Medium",
        children: "No",
        otherPets: "Yes",
        adStatus: "approved",
        adoptStatus: "reserved",
        desc: "A sweet dog that needs a caring adopter.",
        image: ""
    },
    {
        id: 3,
        name: "Bebe",
        type: "bird",
        typeLabel: "Bird",
        breed: "Cockatiel",
        age: "6 months",
        gender: "Female",
        color: "Yellow",
        health: "Healthy",
        vacc: "No",
        neuter: "—",
        vaccDate: "",
        problems: "",
        traits: "Quiet, friendly",
        energy: "Low",
        children: "Yes",
        otherPets: "Yes",
        adStatus: "pending",
        adoptStatus: "available",
        desc: "A lovely bird with a gentle temperament.",
        image: ""
    },
    {
        id: 4,
        name: "Lulu",
        type: "cat",
        typeLabel: "Cat",
        breed: "Local Mix",
        age: "3 years",
        gender: "Female",
        color: "White",
        health: "Healthy",
        vacc: "Yes",
        neuter: "Yes",
        vaccDate: "2026-01-20",
        problems: "",
        traits: "Friendly, playful",
        energy: "Medium",
        children: "Yes",
        otherPets: "Yes",
        adStatus: "approved",
        adoptStatus: "adopted",
        desc: "Very social and playful cat.",
        image: ""
    }
];

let notifications = [
    { ico: "🛒", bg: "var(--gp)", title: "New order received", sub: "Sarah Al-Khaldi — 3 items — 47.50 JOD", time: "20 minutes ago", unread: true },
    { ico: "⭐", bg: "var(--orap)", title: "New review on Royal Canin", sub: 'Sarah M. — ★★★★★ — "Excellent product"', time: "1 hour ago", unread: true },
    { ico: "✅", bg: "var(--bluep)", title: "Order confirmed", sub: "Ahmad Mansour — 22.00 JOD", time: "3 hours ago", unread: false },
    { ico: "📉", bg: "var(--redp)", title: "Low stock alert", sub: "Anti-Flea Shampoo has only 2 units left", time: "5 hours ago", unread: true },
    { ico: "💳", bg: "var(--gu)", title: "Monthly payment received", sub: "60.00 JOD — Professional Plan", time: "1 day ago", unread: false }
];

const payHist = [
    { type: "Monthly Subscription", plan: "Professional", amount: "60.00", date: "1 Mar 2026" },
    { type: "Monthly Subscription", plan: "Professional", amount: "60.00", date: "1 Feb 2026" },
    { type: "Monthly Subscription", plan: "Professional", amount: "60.00", date: "1 Jan 2026" },
    { type: "Monthly Subscription", plan: "Basic", amount: "25.00", date: "1 Dec 2025" }
];

const typeIco = { cat: "🐈", dog: "🐕", bird: "🐦", other: "🐾" };

const adMap = {
    approved: { lbl: "Published", cls: "b-green" },
    pending: { lbl: "Pending", cls: "b-yellow" },
    rejected: { lbl: "Rejected", cls: "b-red" }
};

const adoptMap = {
    available: { lbl: "Available", cls: "b-blue" },
    reserved: { lbl: "Reserved", cls: "b-ora" },
    adopted: { lbl: "Adopted ✓", cls: "b-green" }
};

const statusMap = {
    pending: { lbl: "Pending", cls: "b-ora" },
    confirmed: { lbl: "Confirmed", cls: "b-blue" },
    preparing: { lbl: "Preparing", cls: "b-purple" },
    shipped: { lbl: "Shipped", cls: "b-teal" },
    delivered: { lbl: "Delivered", cls: "b-green" },
    cancelled: { lbl: "Cancelled", cls: "b-red" },
    active: { lbl: "Active", cls: "b-green" },
    out: { lbl: "Out of Stock", cls: "b-red" },
    hidden: { lbl: "Hidden", cls: "b-grey" },
    available: { lbl: "Available", cls: "b-blue" },
    reserved: { lbl: "Reserved", cls: "b-ora" },
    adopted: { lbl: "Adopted", cls: "b-green" }
};

function money(v) {
    return `${Number(v).toFixed(2)} JOD`;
}

function statusBadge(status) {
    const st = statusMap[status] || { lbl: status, cls: "b-grey" };
    return `<span class="badge ${st.cls}">${st.lbl}</span>`;
}

function setText(id, value) {
    const el = document.getElementById(id);
    if (el) el.textContent = value;
}

function setHTML(id, value) {
    const el = document.getElementById(id);
    if (el) el.innerHTML = value;
}

/* =========================
   IMAGE HELPERS
========================= */
function getPreviewElements(type) {
    if (type === "product") {
        return {
            input:
                document.getElementById("newProductImage") ||
                document.getElementById("productImageInput"),
            preview:
                document.getElementById("newProductPreview") ||
                document.querySelector("#newProductPreviewWrap img") ||
                document.querySelector(".product-preview-img"),
            previewWrap:
                document.getElementById("newProductPreviewWrap") ||
                document.querySelector(".product-preview-wrap")
        };
    }

    return {
        input:
            document.getElementById("adoptPetImage") ||
            document.getElementById("petImage") ||
            document.getElementById("imageInput"),
        preview:
            document.getElementById("adoptPetPreview") ||
            document.querySelector("#adoptPetPreviewWrap img") ||
            document.querySelector(".preview-wrap .preview-img") ||
            document.querySelector(".adoption-preview-img"),
        previewWrap:
            document.getElementById("adoptPetPreviewWrap") ||
            document.querySelector(".preview-wrap") ||
            document.querySelector(".adoption-preview-wrap")
    };
}

function bindImageUploads() {
    const productEls = getPreviewElements("product");
    if (productEls.input) {
        productEls.input.addEventListener("change", (e) => {
            const file = e.target.files?.[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = function (evt) {
                productImageData = evt.target.result;
                if (productEls.preview) productEls.preview.src = productImageData;
                if (productEls.previewWrap) productEls.previewWrap.style.display = "block";
            };
            reader.readAsDataURL(file);
        });
    }

    const adoptionEls = getPreviewElements("adoption");
    if (adoptionEls.input) {
        adoptionEls.input.addEventListener("change", (e) => {
            const file = e.target.files?.[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = function (evt) {
                adoptionImageData = evt.target.result;
                if (adoptionEls.preview) adoptionEls.preview.src = adoptionImageData;
                if (adoptionEls.previewWrap) adoptionEls.previewWrap.style.display = "block";
            };
            reader.readAsDataURL(file);
        });
    }
}

function resetProductImagePreview() {
    const { input, preview, previewWrap } = getPreviewElements("product");
    if (preview) preview.src = "";
    if (previewWrap) previewWrap.style.display = "none";
    if (input) input.value = "";
    productImageData = "";
}

function resetAdoptionImagePreview() {
    const { input, preview, previewWrap } = getPreviewElements("adoption");
    if (preview) preview.src = "";
    if (previewWrap) previewWrap.style.display = "none";
    if (input) input.value = "";
    adoptionImageData = "";
}

function petVisualHTML(p, size = "large") {
    if (p.image) {
        if (size === "small") {
            return `<img src="${p.image}" alt="${p.name}" style="width:100%;height:100%;object-fit:cover;border-radius:9px;">`;
        }
        return `<img src="${p.image}" alt="${p.name}" style="width:100%;height:100%;object-fit:cover;">`;
    }
    return typeIco[p.type] || "🐾";
}

/* =========================
   DASHBOARD
========================= */
function updateDashboardStats() {
    const totalSales = orders.filter(o => o.status !== "cancelled").reduce((s, o) => s + o.total, 0);
    const totalOrders = orders.length;
    const newOrders = orders.filter(o => o.status === "pending").length;
    const completed = orders.filter(o => o.status === "delivered").length;
    const lowStock = products.filter(p => p.stock <= 2).length;

    setText("salesKpi", Math.round(totalSales));
    setText("ordersKpi", totalOrders);
    setText("newOrdersKpi", newOrders);
    setText("completedOrdersKpi", completed);
    setText("productsKpi", products.length);
    setText("stockTrendText", `▼ ${lowStock} low stock`);

    setText("productsCountBadge", products.length);
    setText("ordersCountBadge", newOrders);
    setText("reviewsCountBadge", reviews.length);
    setText("notifCountBadge", notifications.filter(n => n.unread).length);
}

function renderBestSellers() {
    const holder = document.getElementById("bestSellersList");
    if (!holder) return;

    const counts = {};
    orders.forEach(o =>
        o.items.forEach(item => {
            const name = item.split(" × ")[0];
            const qty = Number(item.split(" × ")[1] || 1);
            counts[name] = (counts[name] || 0) + qty;
        })
    );

    const sorted = Object.entries(counts).sort((a, b) => b[1] - a[1]).slice(0, 4);

    holder.innerHTML = sorted.map((item, i) => `
    <div class="bs-item">
      <div class="bs-rank ${i === 0 ? "gold" : ""}">${i + 1}</div>
      <div>
        <div class="bs-name">${item[0]}</div>
        <div class="bs-sales">${item[1]} sold</div>
      </div>
      <div class="bs-amt">${item[1]}</div>
    </div>
  `).join("");
}

function renderTodayOrders() {
    const body = document.getElementById("todayOrdersBody");
    if (!body) return;

    body.innerHTML = orders.slice(0, 3).map(o => `
    <tr onclick="openOrder('${o.id}')">
      <td><strong>#${o.id}</strong></td>
      <td>
        <div style="display:flex;align-items:center;gap:.5rem">
          <div class="cust-ava">${o.cust[0]}</div>${o.cust}
        </div>
      </td>
      <td><span class="badge b-grey">${o.prods} item(s)</span></td>
      <td><strong>${money(o.total)}</strong></td>
      <td>${statusBadge(o.status)}</td>
      <td><button class="btn btn-sm btn-o" onclick="event.stopPropagation();openOrder('${o.id}')">Details</button></td>
    </tr>
  `).join("");
}

/* =========================
   PRODUCTS
========================= */
function renderProducts() {
    const productsBody = document.getElementById("productsBody");
    if (!productsBody) return;

    const search = (document.getElementById("productSearch")?.value || "").toLowerCase().trim();
    const cat = document.getElementById("productCategoryFilter")?.value || "all";
    const status = document.getElementById("productStatusFilter")?.value || "all";

    const list = products.filter(p => {
        const okSearch = !search || `${p.name} ${p.cat}`.toLowerCase().includes(search);
        const okCat = cat === "all" || p.cat === cat;
        const okStatus = status === "all" || p.status === status;
        return okSearch && okCat && okStatus;
    });

    setText("productsTabCount", products.length);

    productsBody.innerHTML = list.map((p, i) => {
        const pct = Math.round((p.stock / p.maxStock) * 100);
        const barCls = pct === 0 ? "low" : pct < 30 ? "mid" : "";

        return `
      <tr>
        <td>
          <div class="prod-info">
            <div class="prod-img">
              ${p.image
                ? `<img src="${p.image}" alt="${p.name}" style="width:100%;height:100%;object-fit:cover;border-radius:9px;">`
                : p.emoji
            }
            </div>
            <div>
              <div class="prod-name">${p.name}</div>
              <div class="prod-cat">${p.cat}</div>
            </div>
          </div>
        </td>
        <td><strong>${money(p.price)}</strong></td>
        <td>
          <div style="display:flex;align-items:center;gap:.6rem">
            <div style="font-size:.8rem;font-weight:700;color:${pct < 20 ? "var(--red)" : pct < 50 ? "var(--ora)" : "var(--gd)"}">${p.stock}</div>
            <div class="stock-bar">
              <div class="stock-fill ${barCls}" style="width:${pct}%"></div>
            </div>
          </div>
        </td>
        <td><span class="badge b-grey">${p.cat}</span></td>
        <td>${statusBadge(p.status)}</td>
        <td><div class="toggle ${p.status !== "hidden" ? "on" : ""}" onclick="event.stopPropagation();toggleVisibility(${i})"></div></td>
        <td>
          <div style="display:flex;gap:.4rem">
            <button class="btn btn-sm btn-o" onclick="editProduct(${i})">✏️</button>
            <button class="btn btn-sm btn-red" onclick="deleteProduct(${i})">🗑</button>
          </div>
        </td>
      </tr>
    `;
    }).join("") || `<tr><td colspan="7">No products found.</td></tr>`;
}

function editProduct(i) {
    const p = products[i];
    if (!p) return;

    editingProductIndex = i;

    const name = document.getElementById("newProductName");
    const price = document.getElementById("newProductPrice");
    const stock = document.getElementById("newProductStock");
    const category = document.getElementById("newProductCategory");
    const status = document.getElementById("newProductStatus");

    if (name) name.value = p.name;
    if (price) price.value = p.price;
    if (stock) stock.value = p.stock;
    if (category) category.value = p.cat;
    if (status) status.value = p.status === "out" ? "active" : p.status;

    productImageData = p.image || "";

    const { input, preview, previewWrap } = getPreviewElements("product");
    if (preview && p.image) {
        preview.src = p.image;
        if (previewWrap) previewWrap.style.display = "block";
    } else {
        if (preview) preview.src = "";
        if (previewWrap) previewWrap.style.display = "none";
    }

    if (input) input.value = "";

    openModal("addProductModal");
}

function addProduct() {
    const name = document.getElementById("newProductName")?.value.trim() || "";
    const price = Number(document.getElementById("newProductPrice")?.value);
    const stock = Number(document.getElementById("newProductStock")?.value);
    const cat = document.getElementById("newProductCategory")?.value || "";
    const status = document.getElementById("newProductStatus")?.value || "active";

    if (!name || !cat || (!price && price !== 0) || !Number.isFinite(stock)) {
        alert("Please fill in all required product fields.");
        return;
    }

    const productData = {
        name,
        cat,
        price,
        stock,
        maxStock: Math.max(stock, 20),
        status: stock === 0 ? "out" : status,
        emoji: "📦",
        image: productImageData || ""
    };

    if (editingProductIndex !== null) {
        products[editingProductIndex] = {
            ...products[editingProductIndex],
            ...productData
        };
    } else {
        products.unshift(productData);
    }

    closeModal("addProductModal");

    ["newProductName", "newProductPrice", "newProductStock", "newProductCategory"].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = "";
    });

    const statusEl = document.getElementById("newProductStatus");
    if (statusEl) statusEl.value = "active";

    resetProductImagePreview();
    editingProductIndex = null;

    renderProducts();
    updateDashboardStats();
}

function toggleVisibility(i) {
    if (!products[i]) return;
    products[i].status = products[i].status === "hidden" ? "active" : "hidden";
    renderProducts();
    updateDashboardStats();
}

function deleteProduct(i) {
    if (!products[i]) return;
    if (confirm("Delete this product?")) {
        products.splice(i, 1);
        renderProducts();
        updateDashboardStats();
    }
}

function renderCategories() {
    const grid = document.getElementById("categoriesGrid");
    if (!grid) return;

    grid.innerHTML = categories.map((c, i) => `
    <div class="card" style="border-radius:12px; background:var(--surface-2);">
   
      <div class="card-body" style="padding:.9rem;display:flex;align-items:center;justify-content:space-between">
        <div style="display:flex;align-items:center;gap:.6rem">
          <span style="font-size:1.2rem">${c.icon}</span>
          <div>
            <div style="font-size:.84rem;font-weight:700">${c.name}</div>
            <div style="font-size:.72rem;color:var(--muted)">${c.count} products</div>
          </div>
        </div>
        <div style="display:flex;gap:.4rem">
          <button class="btn btn-sm btn-o">✏️</button>
          <button class="btn btn-sm btn-red" onclick="deleteCategory(${i})">🗑</button>
        </div>
      </div>
    </div>
  `).join("") + `
    <div class="card" style="border-radius:12px;border:2px dashed var(--border);background:var(--gu);cursor:pointer" onclick="openModal('addCatModal')">
      <div class="card-body" style="padding:.9rem;text-align:center;color:var(--muted);font-size:.82rem;font-weight:700">+ Add New Category</div>
    </div>
  `;
}

function addCategory() {
    const name = document.getElementById("newCategoryName")?.value.trim() || "";
    const icon = document.getElementById("newCategoryIcon")?.value.trim() || "📦";

    if (!name) {
        alert("Please enter a category name.");
        return;
    }

    categories.push({ name, icon, count: 0 });
    closeModal("addCatModal");

    const nameEl = document.getElementById("newCategoryName");
    const iconEl = document.getElementById("newCategoryIcon");
    if (nameEl) nameEl.value = "";
    if (iconEl) iconEl.value = "";

    renderCategories();
}

function deleteCategory(i) {
    if (!categories[i]) return;
    if (confirm("Delete this category?")) {
        categories.splice(i, 1);
        renderCategories();
    }
}

/* =========================
   ORDERS
========================= */
let activeOrderFilter = "all";

function filterOrders(status) {
    activeOrderFilter = status;
    nav("orders");
    renderOrders();
}

function renderOrders() {
    const ordersBody = document.getElementById("ordersBody");
    if (!ordersBody) return;

    const search = (document.getElementById("orderSearch")?.value || "").toLowerCase().trim();
    const selectStatus = document.getElementById("orderStatusSelect")?.value || "all";
    const finalStatus = activeOrderFilter !== "all" ? activeOrderFilter : selectStatus;

    const list = orders.filter(o => {
        const okSearch = !search || `${o.id} ${o.cust}`.toLowerCase().includes(search);
        const okStatus = finalStatus === "all" || o.status === finalStatus;
        return okSearch && okStatus;
    });

    setText("allOrdersStat", orders.length);
    setText("pendingOrdersStat", orders.filter(o => o.status === "pending").length);
    setText("confirmedOrdersStat", orders.filter(o => o.status === "confirmed").length);
    setText("preparingOrdersStat", orders.filter(o => o.status === "preparing").length);
    setText("shippedOrdersStat", orders.filter(o => o.status === "shipped").length);
    setText("deliveredOrdersStat", orders.filter(o => o.status === "delivered").length);

    ordersBody.innerHTML = list.map(o => `
    <tr onclick="openOrder('${o.id}')">
      <td><strong>#${o.id}</strong></td>
      <td>
        <div style="display:flex;align-items:center;gap:.5rem">
          <div class="cust-ava">${o.cust[0]}</div>${o.cust}
        </div>
      </td>
      <td style="color:var(--muted);font-size:.78rem">${o.date}</td>
      <td><span class="badge b-grey">${o.prods} item(s)</span></td>
      <td><strong>${money(o.total)}</strong></td>
      <td>${statusBadge(o.status)}</td>
      <td><button class="btn btn-sm btn-o" onclick="event.stopPropagation();openOrder('${o.id}')">Details</button></td>
    </tr>
  `).join("") || `<tr><td colspan="7">No orders found.</td></tr>`;
}

function openOrder(id) {
    const o = orders.find(x => x.id === id);
    const body = document.getElementById("orderModalBody");
    if (!o || !body) return;

    body.innerHTML = `
    <div class="grid-2" style="margin-bottom:1rem">
      <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem">
        <div style="font-size:.72rem;font-weight:800;color:var(--muted);margin-bottom:.6rem">Customer Information</div>
        <div style="font-size:.88rem;font-weight:700">${o.cust}</div>
        <div style="font-size:.78rem;color:var(--muted);margin-top:3px">📞 ${o.phone}</div>
        <div style="font-size:.78rem;color:var(--muted)">📧 ${o.email}</div>
        <div style="font-size:.78rem;color:var(--muted)">📍 ${o.address}</div>
      </div>
      <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem">
        <div style="font-size:.72rem;font-weight:800;color:var(--muted);margin-bottom:.6rem">Order Details</div>
        <div style="font-size:.78rem;color:var(--muted)">📅 ${o.date}</div>
        <div style="font-size:.82rem;margin-top:6px"><strong>Order ID:</strong> #${o.id}</div>
        <div style="font-size:.82rem;margin-top:6px"><strong>Total:</strong> ${money(o.total)}</div>
        <div style="font-size:.82rem;margin-top:6px"><strong>Status:</strong> ${statusMap[o.status].lbl}</div>
      </div>
    </div>
    <div class="card" style="box-shadow:none">
      <div class="card-hd"><span class="card-title">🛒 Ordered Items</span></div>
      <div class="card-body">${o.items.map(item => `<div style="padding:.55rem 0;border-bottom:1px solid var(--b)">${item}</div>`).join("")}</div>
    </div>
  `;

    openModal("orderModal");
}

/* =========================
   CUSTOMERS
========================= */
function renderCustomers() {
    const body = document.getElementById("customersBody");
    if (!body) return;

    const search = (document.getElementById("customerSearch")?.value || "").toLowerCase().trim();
    const sort = document.getElementById("customerSort")?.value || "spending";

    let list = customers.filter(c => !search || `${c.name} ${c.email}`.toLowerCase().includes(search));

    if (sort === "spending") list = [...list].sort((a, b) => b.spent - a.spent);
    if (sort === "newest") list = [...list].sort((a, b) => b.orders - a.orders);
    if (sort === "oldest") list = [...list].sort((a, b) => a.orders - b.orders);

    body.innerHTML = list.map(c => `
    <tr>
      <td>
        <div style="display:flex;align-items:center;gap:.6rem">
          <div class="cust-ava">${c.name[0]}</div>
          <div><div style="font-weight:700">${c.name}</div></div>
        </div>
      </td>
      <td style="color:var(--muted);font-size:.8rem">${c.email}</td>
      <td style="font-size:.82rem">${c.phone}</td>
      <td><span class="badge b-blue">${c.orders} orders</span></td>
      <td><strong style="color:var(--gd)">${money(c.spent)}</strong></td>
      <td style="color:var(--muted);font-size:.78rem">${c.last}</td>
    </tr>
  `).join("") || `<tr><td colspan="6">No customers found.</td></tr>`;
}

/* =========================
   REVIEWS
========================= */
function renderReviews() {
    const listWrap = document.getElementById("reviewsList");
    if (!listWrap) return;

    const search = (document.getElementById("reviewSearch")?.value || "").toLowerCase().trim();
    const rating = document.getElementById("reviewRatingFilter")?.value || "all";
    const reply = document.getElementById("reviewReplyFilter")?.value || "all";

    const list = reviews.filter(r => {
        const okSearch = !search || `${r.user} ${r.prod} ${r.text}`.toLowerCase().includes(search);
        const okRating = rating === "all" || String(r.rating) === rating;
        const okReply = reply === "all" || (reply === "replied" ? r.replied : !r.replied);
        return okSearch && okRating && okReply;
    });

    setText("totalReviewsStat", reviews.length);
    setText("pendingRepliesStat", reviews.filter(r => !r.replied).length);

    listWrap.innerHTML = list.map((r, i) => `
    <div class="review-item">
      <div class="review-hd">
        <div class="review-user">
          <div class="review-ava">${r.user[0]}</div>
          <div>
            <div class="review-name">${r.user}</div>
            <div class="review-prod">On: ${r.prod}</div>
          </div>
        </div>
        <div style="text-align:right">
          <div class="stars">${"★".repeat(r.rating)}${"☆".repeat(5 - r.rating)}</div>
          <div style="font-size:.7rem;color:var(--muted);margin-top:2px">${r.time}</div>
        </div>
      </div>
      <div class="review-body">${r.text}</div>
      <div class="review-actions">
        ${r.replied ? `<span class="badge b-green">✓ Replied</span>` : `<button class="btn btn-sm btn-p" onclick="openReply(${i})">💬 Reply</button>`}
        <button class="btn btn-sm btn-red" onclick="flagReview(${i})">⚑ Flag</button>
      </div>
    </div>
  `).join("") || `<div class="card-body">No reviews found.</div>`;
}

function openReply(i) {
    window.currentReviewIndex = i;
    const replyText = document.getElementById("replyText");
    if (replyText) replyText.value = "";
    openModal("replyModal");
}

function submitReply() {
    if (window.currentReviewIndex === undefined) return;

    const replyText = document.getElementById("replyText");
    if (!replyText || !replyText.value.trim()) {
        alert("Please write a reply first.");
        return;
    }

    reviews[window.currentReviewIndex].replied = true;
    closeModal("replyModal");
    renderReviews();
    updateDashboardStats();
}

function flagReview(i) {
    if (!reviews[i]) return;
    alert(`Review from ${reviews[i].user} was flagged.`);
}

/* =========================
   NOTIFICATIONS
========================= */
function renderNotifications() {
    const list = document.getElementById("notifList");
    if (!list) return;

    setText("notifUnreadText", notifications.filter(n => n.unread).length);

    list.innerHTML = notifications.map((n, i) => `
    <div class="notif-item ${n.unread ? "unread" : ""}" onclick="toggleNotification(${i})">
      <div class="notif-ico" style="background:${n.bg}">${n.ico}</div>
      <div class="notif-txt">
        <div class="notif-title">${n.title}</div>
        <div class="notif-sub">${n.sub}</div>
        <div class="notif-time">${n.time}</div>
      </div>
      ${n.unread ? '<div class="unread-dot"></div>' : ""}
    </div>
  `).join("");
}

function toggleNotification(i) {
    if (!notifications[i]) return;
    notifications[i].unread = !notifications[i].unread;
    renderNotifications();
    updateDashboardStats();
}

function markNotificationsRead() {
    notifications.forEach(n => {
        n.unread = false;
    });
    renderNotifications();
    updateDashboardStats();
}

/* =========================
   SUBSCRIPTION
========================= */
function renderPayHist() {
    const list = document.getElementById("payHistList");
    if (!list) return;

    list.innerHTML = payHist.map(p => `
    <div class="pay-hist-item">
      <div class="pay-hist-left">
        <div class="pay-hist-ico" style="background:var(--gp)">💳</div>
        <div>
          <div style="font-size:.84rem;font-weight:700">${p.type} — ${p.plan}</div>
          <div style="font-size:.75rem;color:var(--muted)">${p.date}</div>
        </div>
      </div>
      <div style="display:flex;align-items:center;gap:.8rem">
        <strong style="font-family:'Readex Pro';color:var(--gd)">${p.amount} JOD</strong>
        <span class="badge b-green">✓ Paid</span>
      </div>
    </div>
  `).join("");
}

/* =========================
   CHART
========================= */
function initChart() {
    const ctx = document.getElementById("salesChart");
    if (!ctx || typeof Chart === "undefined") return;

    new Chart(ctx, {
        type: "line",
        data: {
            labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            datasets: [{
                label: "Sales (JOD)",
                data: [820, 950, 1100, 870, 1250, 1400, 1180, 1320, 980, 1450, 1600, 1840],
                borderColor: "#5a8f38",
                backgroundColor: "rgba(90,143,56,.08)",
                borderWidth: 2.5,
                pointBackgroundColor: "#3d6b27",
                pointRadius: 4,
                pointHoverRadius: 6,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: context => `${context.parsed.y} JOD`
                    }
                }
            },
            scales: {
                y: {
                    grid: { color: "rgba(90,143,56,.07)" },
                    ticks: { color: "#8a9e72" }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: "#8a9e72" }
                }
            }
        }
    });
}

/* =========================
   ADOPTION
========================= */
function openAdoptionAddPage() {
    const manage = document.getElementById("adoptionManagePage");
    const add = document.getElementById("adoptionAddPage");
    if (manage) manage.style.display = "none";
    if (add) add.style.display = "block";
}

function openAdoptionManagePage() {
    const manage = document.getElementById("adoptionManagePage");
    const add = document.getElementById("adoptionAddPage");
    if (manage) manage.style.display = "block";
    if (add) add.style.display = "none";
}

function toggleAdoptionView() {
    adoptionViewMode = adoptionViewMode === "cards" ? "table" : "cards";

    const cardView = document.getElementById("adoptionCardView");
    const tableView = document.getElementById("adoptionTableView");

    if (cardView) cardView.style.display = adoptionViewMode === "cards" ? "block" : "none";
    if (tableView) tableView.style.display = adoptionViewMode === "table" ? "block" : "none";
}

function toggleVaccDate() {
    const value = document.getElementById("petVaccinated")?.value;
    const row = document.getElementById("vaccRow");
    if (row) row.style.display = value === "Yes" ? "block" : "none";
}

function getAdoptionFilteredData(viewType = "cards") {
    const searchInput = document.getElementById(viewType === "cards" ? "adoptionSearchCards" : "adoptionSearchTable");
    const statusInput = document.getElementById(viewType === "cards" ? "adoptionStatusCards" : "adoptionStatusTable");

    const search = (searchInput?.value || "").trim().toLowerCase();
    const status = statusInput?.value || "all";

    return adoptionPets.filter(p => {
        const matchesSearch = `${p.name} ${p.typeLabel} ${p.breed}`.toLowerCase().includes(search);
        const matchesStatus = status === "all" || p.adoptStatus === status || p.adStatus === status;
        return matchesSearch && matchesStatus;
    });
}

function renderAdoptionStats() {
    setText("adoptTotalCount", adoptionPets.length);
    setText("adoptAvailableCount", adoptionPets.filter(p => p.adoptStatus === "available").length);
    setText("adoptPendingCount", adoptionPets.filter(p => p.adStatus === "pending").length);
    setText("adoptAdoptedCount", adoptionPets.filter(p => p.adoptStatus === "adopted").length);
}

function renderPetCards() {
    const list = getAdoptionFilteredData("cards");
    const grid = document.getElementById("petCardsGrid");
    if (!grid) return;

    if (!list.length) {
        grid.innerHTML = `<div style="grid-column:1/-1;padding:1rem;text-align:center;color:var(--muted)">No pets found.</div>`;
        return;
    }

    grid.innerHTML = list.map(p => `
    <div class="pet-card" onclick="openPetDetail(${p.id})">
      <div class="pet-card-img">
        ${petVisualHTML(p, "large")}
        <div class="pet-card-status">
          <span class="badge ${adoptMap[p.adoptStatus].cls}">${adoptMap[p.adoptStatus].lbl}</span>
        </div>
      </div>
      <div class="pet-card-body">
        <div class="pet-card-name">${p.name}</div>
        <div class="pet-card-meta">${p.typeLabel} — ${p.age} — ${p.gender}</div>
        <span class="badge ${adMap[p.adStatus].cls}">${adMap[p.adStatus].lbl}</span>
        <div class="pet-card-footer">
          <button class="btn btn-sm btn-o" onclick="event.stopPropagation();openPetDetail(${p.id})">View</button>
          <button class="btn btn-sm btn-red" onclick="event.stopPropagation();deleteAdoptionPet(${p.id})">Delete</button>
        </div>
      </div>
    </div>
  `).join("");
}

function renderPetTable() {
    const list = getAdoptionFilteredData("table");
    const body = document.getElementById("petTableBody");
    if (!body) return;

    if (!list.length) {
        body.innerHTML = `<tr><td colspan="8" style="text-align:center;color:var(--muted)">No pets found.</td></tr>`;
        return;
    }

    body.innerHTML = list.map(p => `
    <tr onclick="openPetDetail(${p.id})">
      <td>
        <div style="display:flex;align-items:center;gap:.6rem">
          <div style="width:36px;height:36px;border-radius:9px;background:var(--gp);border:1px solid var(--b);display:flex;align-items:center;justify-content:center;font-size:1.3rem;flex-shrink:0;overflow:hidden">
            ${petVisualHTML(p, "small")}
          </div>
          <strong>${p.name}</strong>
        </div>
      </td>
      <td>${p.typeLabel}</td>
      <td style="color:var(--muted)">${p.age}</td>
      <td>${p.gender}</td>
      <td><span class="badge ${p.health === "Healthy" ? "b-green" : "b-ora"}">${p.health}</span></td>
      <td><span class="badge ${adoptMap[p.adoptStatus].cls}">${adoptMap[p.adoptStatus].lbl}</span></td>
      <td><span class="badge ${adMap[p.adStatus].cls}">${adMap[p.adStatus].lbl}</span></td>
      <td onclick="event.stopPropagation()">
        <div style="display:flex;gap:.35rem;flex-wrap:wrap">
          <button class="btn btn-sm btn-o" onclick="openPetDetail(${p.id})">View</button>
          <button class="btn btn-sm btn-red" onclick="deleteAdoptionPet(${p.id})">Delete</button>
        </div>
      </td>
    </tr>
  `).join("");
}

function openPetDetail(id) {
    const p = adoptionPets.find(x => x.id === id);
    const title = document.getElementById("petModalTitle");
    const body = document.getElementById("petModalBody");
    if (!p || !title || !body) return;

    title.textContent = `🐾 ${p.name} — ${p.typeLabel}`;

    body.innerHTML = `
    <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem">
      <div>
        <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem;margin-bottom:.8rem">
          <div style="width:100%;height:220px;border-radius:14px;overflow:hidden;background:var(--gp);display:flex;align-items:center;justify-content:center;font-size:3rem;margin-bottom:1rem">
            ${petVisualHTML(p, "large")}
          </div>

          <div style="font-size:.68rem;font-weight:800;color:var(--muted);letter-spacing:1px;margin-bottom:.6rem">🐾 Pet Information</div>
          ${[
            ["Name", p.name],
            ["Type", p.typeLabel],
            ["Breed", p.breed],
            ["Age", p.age],
            ["Gender", p.gender],
            ["Color", p.color || "-"]
        ].map(([k, v]) => `
            <div style="display:flex;justify-content:space-between;padding:.3rem 0;border-bottom:1px solid var(--b);font-size:.81rem">
              <span style="color:var(--muted)">${k}</span><strong>${v}</strong>
            </div>
          `).join("")}
        </div>

        <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem">
          <div style="font-size:.68rem;font-weight:800;color:var(--muted);letter-spacing:1px;margin-bottom:.6rem">❤️ Health</div>
          ${[
            ["Status", p.health],
            ["Vaccinated", p.vacc],
            ["Neutered / Spayed", p.neuter],
            ["Vaccination Date", p.vaccDate || "-"],
            ["Problems", p.problems || "-"]
        ].map(([k, v]) => `
            <div style="display:flex;justify-content:space-between;padding:.3rem 0;border-bottom:1px solid var(--b);font-size:.81rem">
              <span style="color:var(--muted)">${k}</span><strong>${v}</strong>
            </div>
          `).join("")}
        </div>
      </div>

      <div>
        <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem;margin-bottom:.8rem">
          <div style="font-size:.68rem;font-weight:800;color:var(--muted);letter-spacing:1px;margin-bottom:.6rem">🧠 Behaviour</div>
          <div style="font-size:.8rem;color:var(--textm);margin-bottom:.4rem">${p.traits || "-"}</div>
          ${[
            ["Energy", p.energy || "-"],
            ["Good with children", p.children || "-"],
            ["Good with pets", p.otherPets || "-"]
        ].map(([k, v]) => `
            <div style="display:flex;justify-content:space-between;padding:.3rem 0;border-bottom:1px solid var(--b);font-size:.81rem">
              <span style="color:var(--muted)">${k}</span><strong>${v}</strong>
            </div>
          `).join("")}
        </div>

        <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem">
          <div style="font-size:.68rem;font-weight:800;color:var(--muted);letter-spacing:1px;margin-bottom:.6rem">📊 Status</div>
          <div style="display:flex;gap:.4rem;flex-wrap:wrap;margin-bottom:.7rem">
            <span class="badge ${adMap[p.adStatus].cls}">${adMap[p.adStatus].lbl}</span>
            <span class="badge ${adoptMap[p.adoptStatus].cls}">${adoptMap[p.adoptStatus].lbl}</span>
          </div>
          <div style="font-size:.8rem;color:var(--textm)"><b>Description:</b><br>${p.desc || "-"}</div>
        </div>
      </div>
    </div>
  `;

    openModal("petModal");
}

function addAdoptionPet() {
    const name = document.getElementById("adoptPetName")?.value.trim() || "";
    const type = document.getElementById("adoptPetType")?.value || "";
    const breed = document.getElementById("adoptPetBreed")?.value.trim() || "";
    const age = document.getElementById("adoptPetAge")?.value.trim() || "-";
    const status = document.getElementById("adoptPetStatus")?.value || "available";

    if (!name || !type || !breed) {
        alert("Please fill in all required adoption fields.");
        return;
    }

    adoptionPets.unshift({
        id: Date.now(),
        name,
        type: type.toLowerCase(),
        typeLabel: type,
        breed,
        age,
        gender: "-",
        color: "-",
        health: "Healthy",
        vacc: "No",
        neuter: "—",
        vaccDate: "",
        problems: "",
        traits: "",
        energy: "-",
        children: "-",
        otherPets: "-",
        adStatus: "pending",
        adoptStatus: status,
        desc: "",
        image: adoptionImageData || ""
    });

    closeModal("addAdoptionModal");

    ["adoptPetName", "adoptPetType", "adoptPetBreed", "adoptPetAge"].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = "";
    });

    const statusEl = document.getElementById("adoptPetStatus");
    if (statusEl) statusEl.value = "available";

    resetAdoptionImagePreview();
    renderAdoptionSection();
}

function deleteAdoptionPet(id) {
    if (!confirm("Delete this adoption listing?")) return;
    adoptionPets = adoptionPets.filter(p => p.id !== id);
    renderAdoptionSection();
}

function submitAdoptionPet() {
    const name = document.getElementById("petName")?.value.trim() || "";
    const type = document.getElementById("petType")?.value || "";
    const breed = document.getElementById("petBreed")?.value.trim() || "";
    const age = document.getElementById("petAge")?.value.trim() || "";
    const gender = document.getElementById("petGender")?.value || "";
    const color = document.getElementById("petColor")?.value.trim() || "";
    const health = document.getElementById("petHealth")?.value || "";
    const neuter = document.getElementById("petNeuter")?.value || "";
    const vacc = document.getElementById("petVaccinated")?.value || "";
    const vaccDate = document.getElementById("petVaccDate")?.value || "";
    const problems = document.getElementById("petProblems")?.value.trim() || "";
    const energy = document.getElementById("petEnergy")?.value || "";
    const children = document.getElementById("petChildren")?.value || "";
    const otherPets = document.getElementById("petOtherPets")?.value || "";
    const desc = document.getElementById("petDesc")?.value.trim() || "";
    const adoptStatus = document.getElementById("petAdoptionStatus")?.value || "available";
    const adStatus = document.getElementById("petListingStatus")?.value || "pending";

    const selectedTraits = [...document.querySelectorAll(".trait-chip.on")].map(t => t.textContent.trim()).join(", ");
    const typeMap = { dog: "Dog", cat: "Cat", bird: "Bird", other: "Other" };

    if (!name || !type || !breed || !age || !gender || !health || !neuter || !vacc) {
        alert("Please fill in all required fields.");
        return;
    }

    adoptionPets.unshift({
        id: Date.now(),
        name,
        type,
        typeLabel: typeMap[type] || "Other",
        breed,
        age,
        gender,
        color,
        health,
        vacc,
        neuter,
        vaccDate,
        problems,
        traits: selectedTraits,
        energy,
        children,
        otherPets,
        adStatus,
        adoptStatus,
        desc,
        image: adoptionImageData || ""
    });

    [
        "petName", "petType", "petBreed", "petAge", "petGender", "petColor",
        "petHealth", "petNeuter", "petVaccinated", "petVaccDate", "petProblems", "petDesc"
    ].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = "";
    });

    const petEnergy = document.getElementById("petEnergy");
    const petChildren = document.getElementById("petChildren");
    const petOtherPets = document.getElementById("petOtherPets");
    const petAdoptionStatus = document.getElementById("petAdoptionStatus");
    const petListingStatus = document.getElementById("petListingStatus");
    const vaccRow = document.getElementById("vaccRow");

    if (petEnergy) petEnergy.value = "Medium";
    if (petChildren) petChildren.value = "Yes";
    if (petOtherPets) petOtherPets.value = "Yes";
    if (petAdoptionStatus) petAdoptionStatus.value = "available";
    if (petListingStatus) petListingStatus.value = "pending";
    if (vaccRow) vaccRow.style.display = "none";

    document.querySelectorAll(".trait-chip").forEach(chip => chip.classList.remove("on"));

    resetAdoptionImagePreview();

    const toast = document.getElementById("toast");
    if (toast) {
        toast.style.display = "flex";
        setTimeout(() => {
            toast.style.display = "none";
        }, 1800);
    }

    openAdoptionManagePage();
    renderAdoptionSection();
    nav("adoption");
}

function setAdoptionMode(mode) {
    adoptionMode = mode;
    const isClinic = mode === "clinic";

    const manageSub = document.getElementById("manageSub");
    const addSub = document.getElementById("addSub");
    const medReportRow = document.getElementById("medReportRow");

    if (manageSub) {
        manageSub.textContent = isClinic
            ? "Manage animals available for adoption from your clinic"
            : "Manage animals available for adoption from your store";
    }

    if (addSub) {
        addSub.textContent = isClinic
            ? "Add a pet from the clinic for adoption"
            : "Add a pet from the store for adoption";
    }

    if (medReportRow) {
        medReportRow.style.display = isClinic ? "flex" : "none";
    }
}

function renderAdoptionSection() {
    renderAdoptionStats();
    renderPetCards();
    renderPetTable();
}

/* =========================
   COMMON UI
========================= */
function nav(page) {
    document.querySelectorAll(".page").forEach(p => p.classList.remove("on"));
    document.querySelectorAll(".nav-item").forEach(n => n.classList.remove("active"));
    document.getElementById("page-" + page)?.classList.add("on");
    document.querySelector(`[data-page="${page}"]`)?.classList.add("active");
}

function switchTab(el, paneId) {
    const card = el.closest(".card, .table-card");
    if (!card) return;

    card.querySelectorAll(".tab").forEach(t => t.classList.remove("on"));
    card.querySelectorAll(".tab-pane").forEach(p => p.classList.remove("on"));
    el.classList.add("on");
    document.getElementById(paneId)?.classList.add("on");
}

function switchSettings(el, paneId) {
    document.querySelectorAll(".settings-nav-item").forEach(i => i.classList.remove("on"));
    document.querySelectorAll(".settings-pane").forEach(p => p.classList.remove("on"));
    el.classList.add("on");
    document.getElementById(paneId)?.classList.add("on");
}

function openModal(id) {
    document.getElementById(id)?.classList.add("on");
}

function closeModal(id) {
    document.getElementById(id)?.classList.remove("on");
}

document.addEventListener("click", e => {
    if (e.target.classList.contains("overlay")) {
        e.target.classList.remove("on");
    }
});

function toggleSidebar() {
    const sidebar = document.querySelector(".sidebar");
    const overlay = document.getElementById("sidebarOverlay");

    if (!sidebar) return;

    sidebar.classList.toggle("on");
    overlay?.classList.toggle("on");
    document.body.classList.toggle("sidebar-open");
}

function closeSidebar() {
    const sidebar = document.querySelector(".sidebar");
    const overlay = document.getElementById("sidebarOverlay");

    sidebar?.classList.remove("on");
    overlay?.classList.remove("on");
    document.body.classList.remove("sidebar-open");
}

function bindEvents() {
    document.getElementById("productSearch")?.addEventListener("input", renderProducts);
    document.getElementById("productCategoryFilter")?.addEventListener("change", renderProducts);
    document.getElementById("productStatusFilter")?.addEventListener("change", renderProducts);

    document.getElementById("orderSearch")?.addEventListener("input", () => {
        activeOrderFilter = "all";
        renderOrders();
    });

    document.getElementById("orderStatusSelect")?.addEventListener("change", () => {
        activeOrderFilter = "all";
        renderOrders();
    });

    document.getElementById("customerSearch")?.addEventListener("input", renderCustomers);
    document.getElementById("customerSort")?.addEventListener("change", renderCustomers);

    document.getElementById("reviewSearch")?.addEventListener("input", renderReviews);
    document.getElementById("reviewRatingFilter")?.addEventListener("change", renderReviews);
    document.getElementById("reviewReplyFilter")?.addEventListener("change", renderReviews);

    document.getElementById("adoptionSearchCards")?.addEventListener("input", renderPetCards);
    document.getElementById("adoptionStatusCards")?.addEventListener("change", renderPetCards);
    document.getElementById("adoptionSearchTable")?.addEventListener("input", renderPetTable);
    document.getElementById("adoptionStatusTable")?.addEventListener("change", renderPetTable);
}

document.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    bindImageUploads();
    updateDashboardStats();

    if (document.getElementById("bestSellersList")) renderBestSellers();
    if (document.getElementById("todayOrdersBody")) renderTodayOrders();
    if (document.getElementById("productsBody")) renderProducts();
    if (document.getElementById("categoriesGrid")) renderCategories();
    if (document.getElementById("ordersBody")) renderOrders();
    if (document.getElementById("customersBody")) renderCustomers();
    if (document.getElementById("reviewsList")) renderReviews();
    if (document.getElementById("page-adoption") || document.getElementById("petCardsGrid") || document.getElementById("petTableBody")) renderAdoptionSection();
    if (document.getElementById("notifList")) renderNotifications();
    if (document.getElementById("payHistList")) renderPayHist();
    if (document.getElementById("salesChart")) initChart();
});