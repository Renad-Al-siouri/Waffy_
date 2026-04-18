/* ============================================================
   AdminStore.js  —  Waffy Store Admin
   كل العمليات مربوطة بالسيرفر عبر fetch
   ============================================================ */

// ── State ────────────────────────────────────────────────────
let products = window.serverProducts || [];
let orders = window.serverOrders || [];
let customers = window.serverCustomers || [];

let editingProductId = null;   // يستخدم p.id وليس index
let activeOrderFilter = "all";
let adoptionViewMode = "cards";
let productImageData = "";
let adoptionImageData = "";

// ── Static / dummy data (لم يُربط بعد بالسيرفر) ─────────────
const categories = [
    { name: "Food", icon: "🍖", count: 0 },
    { name: "Accessories", icon: "🎀", count: 0 },
    { name: "Health Care", icon: "💊", count: 0 },
    { name: "Toys", icon: "🧸", count: 0 }
];

let reviews = [];
let notifications = [];
const payHist = [];

let adoptionPets = [];

// ── Maps ─────────────────────────────────────────────────────
const statusMap = {
    pending: { lbl: "Pending", cls: "b-ora" },
    confirmed: { lbl: "Confirmed", cls: "b-blue" },
    preparing: { lbl: "Preparing", cls: "b-purple" },
    shipped: { lbl: "Shipped", cls: "b-teal" },
    delivered: { lbl: "Delivered", cls: "b-green" },
    cancelled: { lbl: "Cancelled", cls: "b-red" },
    active: { lbl: "Active", cls: "b-green" },
    out: { lbl: "Out of Stock", cls: "b-red" },
    hidden: { lbl: "Hidden", cls: "b-grey" }
};

const orderStatusFlow = ["pending", "confirmed", "preparing", "shipped", "delivered", "cancelled"];

// ── Helpers ──────────────────────────────────────────────────
function money(v) { return `${Number(v).toFixed(2)} JOD`; }

function statusBadge(status) {
    const st = statusMap[status] || { lbl: status, cls: "b-grey" };
    return `<span class="badge ${st.cls}">${st.lbl}</span>`;
}

function setText(id, value) {
    const el = document.getElementById(id);
    if (el) el.textContent = value;
}

function showToast(msg = "✅ Done!") {
    const t = document.getElementById("toast");
    if (!t) return;
    t.textContent = msg;
    t.style.display = "flex";
    setTimeout(() => { t.style.display = "none"; }, 2000);
}

function getAntiForgeryToken() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : "";
}

/* =============================================================
   IMAGE HELPERS
============================================================= */
function getPreviewElements(type) {
    if (type === "product") {
        return {
            input: document.getElementById("newProductImage"),
            preview: document.getElementById("newProductPreview"),
            previewWrap: document.getElementById("newProductPreviewWrap")
        };
    }
    return {
        input: document.getElementById("adoptPetImage") || document.getElementById("petImage"),
        preview: document.getElementById("adoptPetPreview"),
        previewWrap: document.getElementById("adoptPetPreviewWrap")
    };
}

function bindImageUploads() {
    const pEls = getPreviewElements("product");
    if (pEls.input) {
        pEls.input.addEventListener("change", e => {
            const file = e.target.files?.[0];
            if (!file) return;
            const reader = new FileReader();
            reader.onload = evt => {
                productImageData = evt.target.result;
                if (pEls.preview) pEls.preview.src = productImageData;
                if (pEls.previewWrap) pEls.previewWrap.style.display = "block";
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

/* =============================================================
   DASHBOARD KPIs
============================================================= */
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
}

function renderBestSellers() {
    const holder = document.getElementById("bestSellersList");
    if (!holder) return;
    const counts = {};
    orders.forEach(o =>
        (o.items || []).forEach(item => {
            const parts = item.split(" × ");
            const name = parts[0];
            const qty = Number(parts[1] || 1);
            counts[name] = (counts[name] || 0) + qty;
        })
    );
    const sorted = Object.entries(counts).sort((a, b) => b[1] - a[1]).slice(0, 4);
    holder.innerHTML = sorted.length
        ? sorted.map((item, i) => `
            <div class="bs-item">
              <div class="bs-rank ${i === 0 ? "gold" : ""}">${i + 1}</div>
              <div><div class="bs-name">${item[0]}</div><div class="bs-sales">${item[1]} sold</div></div>
              <div class="bs-amt">${item[1]}</div>
            </div>`).join("")
        : `<div style="color:var(--muted);font-size:.82rem;padding:.5rem">No orders yet.</div>`;
}

function renderTodayOrders() {
    const body = document.getElementById("todayOrdersBody");
    if (!body) return;
    body.innerHTML = orders.slice(0, 5).map(o => `
        <tr onclick="openOrder(${o.idOrder})">
          <td><strong>#${o.idOrder}</strong></td>
          <td>
            <div style="display:flex;align-items:center;gap:.5rem">
              <div class="cust-ava">${(o.cust || "?")[0]}</div>${o.cust}
            </div>
          </td>
          <td><span class="badge b-grey">${o.prods} item(s)</span></td>
          <td><strong>${money(o.total)}</strong></td>
          <td>${statusBadge(o.status)}</td>
          <td><button class="btn btn-sm btn-o" onclick="event.stopPropagation();openOrder(${o.idOrder})">Details</button></td>
        </tr>`).join("") || `<tr><td colspan="6" style="text-align:center;color:var(--muted)">No orders yet.</td></tr>`;
}

/* =============================================================
   PRODUCTS — Render
============================================================= */
function renderProducts() {
    const productsBody = document.getElementById("productsBody");
    if (!productsBody) return;

    const search = (document.getElementById("productSearch")?.value || "").toLowerCase().trim();
    const cat = document.getElementById("productCategoryFilter")?.value || "all";
    const status = document.getElementById("productStatusFilter")?.value || "all";

    const list = products.filter(p => {
        const okSearch = !search || `${p.name} ${p.cat}`.toLowerCase().includes(search);
        const okCat = cat === "all" || String(p.cat) === String(cat);
        const okStatus = status === "all" || p.status === status;
        return okSearch && okCat && okStatus;
    });

    setText("productsTabCount", products.length);
    setText("productsCountBadge", products.length);

    productsBody.innerHTML = list.map(p => {
        const maxStock = p.maxStock || Math.max(p.stock * 2, 20);
        const pct = maxStock > 0 ? Math.round((p.stock / maxStock) * 100) : 0;
        const barCls = pct === 0 ? "low" : pct < 30 ? "mid" : "";
        const imgHtml = p.image
            ? `<img src="${p.image}" alt="${p.name}" style="width:100%;height:100%;object-fit:cover;border-radius:9px;">`
            : `<span style="font-size:1.4rem">📦</span>`;

        return `
        <tr>
          <td>
            <div class="prod-info">
              <div class="prod-img">${imgHtml}</div>
              <div>
                <div class="prod-name">${p.name}</div>
                <div class="prod-cat">${p.cat}</div>
              </div>
            </div>
          </td>
          <td><strong>${money(p.price)}</strong>${p.salePrice ? `<br><span style="color:var(--red);font-size:.75rem">${money(p.salePrice)}</span>` : ""}</td>
          <td>
            <div style="display:flex;align-items:center;gap:.6rem">
              <div style="font-size:.8rem;font-weight:700;color:${pct < 20 ? "var(--red)" : pct < 50 ? "var(--ora)" : "var(--gd)"}">${p.stock}</div>
              <div class="stock-bar"><div class="stock-fill ${barCls}" style="width:${pct}%"></div></div>
            </div>
          </td>
          <td><span class="badge b-grey">${p.cat}</span></td>
          <td>${statusBadge(p.status)}</td>
          <td><div class="toggle ${p.status !== "hidden" ? "on" : ""}" onclick="event.stopPropagation();toggleVisibility(${p.id})"></div></td>
          <td>
            <div style="display:flex;gap:.4rem">
              <button class="btn btn-sm btn-o"   onclick="editProduct(${p.id})">✏️</button>
              <button class="btn btn-sm btn-red" onclick="deleteProduct(${p.id})">🗑</button>
            </div>
          </td>
        </tr>`;
    }).join("") || `<tr><td colspan="7" style="text-align:center;color:var(--muted)">No products found.</td></tr>`;
}

/* =============================================================
   PRODUCTS — CRUD (مربوط بالسيرفر)
============================================================= */

/** فتح modal الإضافة في وضع Add */
function openAddProductModal() {
    editingProductId = null;
    // تفريغ الحقول
    ["newProductName", "newProductDescription", "newProductPrice",
        "newProductSalePrice", "newProductStock"].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = "";
        });
    const cat = document.getElementById("newProductCategory");
    const st = document.getElementById("newProductStatus");
    if (cat) cat.value = "";
    if (st) st.value = "active";
    resetProductImagePreview();

    const title = document.querySelector("#addProductModal .modal-title");
    if (title) title.textContent = "📦 Add New Product";
    openModal("addProductModal");
}

/** تحميل بيانات المنتج في modal للتعديل */
function editProduct(productId) {
    const p = products.find(x => x.id === productId);
    if (!p) return;

    editingProductId = productId;

    const set = (id, val) => { const el = document.getElementById(id); if (el) el.value = val ?? ""; };
    set("newProductName", p.name);
    set("newProductDescription", p.description || "");
    set("newProductPrice", p.price);
    set("newProductSalePrice", p.salePrice ?? "");
    set("newProductStock", p.stock);
    set("newProductCategory", p.cat);
    set("newProductStatus", p.status === "out" ? "active" : p.status);

    // صورة
    const { preview, previewWrap, input } = getPreviewElements("product");
    if (p.image) {
        if (preview) preview.src = p.image;
        if (previewWrap) previewWrap.style.display = "block";
    } else {
        if (preview) preview.src = "";
        if (previewWrap) previewWrap.style.display = "none";
    }
    if (input) input.value = "";
    productImageData = "";

    const title = document.querySelector("#addProductModal .modal-title");
    if (title) title.textContent = "✏️ Edit Product";
    openModal("addProductModal");
}

/** حفظ المنتج (إضافة أو تعديل) */
async function addProduct() {
    const name = document.getElementById("newProductName")?.value.trim() || "";
    const description = document.getElementById("newProductDescription")?.value.trim() || "";
    const price = parseFloat(document.getElementById("newProductPrice")?.value) || 0;
    const salePrice = parseFloat(document.getElementById("newProductSalePrice")?.value) || null;
    const stock = parseInt(document.getElementById("newProductStock")?.value) || 0;
    const idCategory = parseInt(document.getElementById("newProductCategory")?.value) || 1;
    const status = document.getElementById("newProductStatus")?.value || "active";
    const imageInput = document.getElementById("newProductImage");

    if (!name || !idCategory) {
        alert("Please fill in all required fields (Name, Category).");
        return;
    }

    const formData = new FormData();
    formData.append("name", name);
    formData.append("description", description);
    formData.append("price", price);
    if (salePrice) formData.append("salePrice", salePrice);
    formData.append("stockQuantity", stock);
    formData.append("idCategory", idCategory);

    if (imageInput?.files?.[0]) {
        formData.append("imageFile", imageInput.files[0]);
    }

    const saveBtn = document.querySelector("#addProductModal .btn-primary");
    if (saveBtn) { saveBtn.disabled = true; saveBtn.textContent = "Saving…"; }

    try {
        let url, method = "POST";
        if (editingProductId !== null) {
            url = `/StoreAdmin/EditProduct`;
            formData.append("id", editingProductId);
        } else {
            url = `/StoreAdmin/AddProduct`;
        }

        const res = await fetch(url, { method, body: formData });
        const data = await res.json();

        if (!data.success) {
            alert(data.message || "Failed to save product.");
            return;
        }

        // تحديث الـ local array
        if (editingProductId !== null) {
            const idx = products.findIndex(x => x.id === editingProductId);
            if (idx !== -1) {
                products[idx] = {
                    ...products[idx],
                    name, description, price,
                    salePrice: salePrice || null,
                    stock,
                    cat: String(idCategory),
                    status: stock === 0 ? "out" : status,
                    image: data.imageUrl || products[idx].image
                };
            }
        } else {
            // إضافة المنتج الجديد في أول القائمة
            products.unshift({
                id: data.id,
                name,
                description,
                price,
                salePrice: salePrice || null,
                stock,
                maxStock: Math.max(stock * 2, 20),
                cat: String(idCategory),
                status: stock === 0 ? "out" : status,
                image: data.imageUrl || ""
            });
        }

        closeModal("addProductModal");
        resetProductImagePreview();
        editingProductId = null;
        renderProducts();
        updateDashboardStats();
        showToast(editingProductId !== null ? "✅ Product updated!" : "✅ Product added!");

    } catch (err) {
        console.error(err);
        alert("Network error. Please try again.");
    } finally {
        if (saveBtn) { saveBtn.disabled = false; saveBtn.textContent = "💾 Save Product"; }
    }
}

/** حذف منتج */
async function deleteProduct(productId) {
    const p = products.find(x => x.id === productId);
    if (!p) return;
    if (!confirm(`Delete "${p.name}"?`)) return;

    try {
        const formData = new FormData();
        formData.append("id", productId);

        const res = await fetch("/StoreAdmin/DeleteProduct", { method: "POST", body: formData });
        const data = await res.json();

        if (!data.success) {
            alert(data.message || "Failed to delete product.");
            return;
        }

        products = products.filter(x => x.id !== productId);
        renderProducts();
        updateDashboardStats();
        showToast("🗑 Product deleted.");

    } catch (err) {
        console.error(err);
        alert("Network error.");
    }
}

/** إظهار/إخفاء منتج */
async function toggleVisibility(productId) {
    const p = products.find(x => x.id === productId);
    if (!p) return;

    try {
        const formData = new FormData();
        formData.append("id", productId);

        const res = await fetch("/StoreAdmin/ToggleVisibility", { method: "POST", body: formData });
        const data = await res.json();

        if (!data.success) {
            alert(data.message || "Failed to toggle visibility.");
            return;
        }

        p.status = data.isVisible ? (p.stock === 0 ? "out" : "active") : "hidden";
        renderProducts();

    } catch (err) {
        console.error(err);
        alert("Network error.");
    }
}

/* =============================================================
   CATEGORIES (local only — لم يُربط بالسيرفر بعد)
============================================================= */
function renderCategories() {
    const grid = document.getElementById("categoriesGrid");
    if (!grid) return;

    grid.innerHTML = categories.map((c, i) => `
        <div class="card" style="border-radius:12px;background:var(--surface-2)">
          <div class="card-body" style="padding:.9rem;display:flex;align-items:center;justify-content:space-between">
            <div style="display:flex;align-items:center;gap:.6rem">
              <span style="font-size:1.2rem">${c.icon}</span>
              <div>
                <div style="font-size:.84rem;font-weight:700">${c.name}</div>
                <div style="font-size:.72rem;color:var(--muted)">${c.count} products</div>
              </div>
            </div>
            <button class="btn btn-sm btn-red" onclick="deleteCategory(${i})">🗑</button>
          </div>
        </div>`).join("") + `
        <div class="card" style="border-radius:12px;border:2px dashed var(--border);background:var(--gu);cursor:pointer" onclick="openModal('addCatModal')">
          <div class="card-body" style="padding:.9rem;text-align:center;color:var(--muted);font-size:.82rem;font-weight:700">+ Add New Category</div>
        </div>`;
}

function addCategory() {
    const name = document.getElementById("newCategoryName")?.value.trim() || "";
    const icon = document.getElementById("newCategoryIcon")?.value.trim() || "📦";
    if (!name) { alert("Please enter a category name."); return; }
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
    if (confirm("Delete this category?")) { categories.splice(i, 1); renderCategories(); }
}

/* =============================================================
   ORDERS — Render
============================================================= */
function filterOrders(status) {
    activeOrderFilter = status;
    renderOrders();
}

function renderOrders() {
    const ordersBody = document.getElementById("ordersBody");
    if (!ordersBody) return;

    const search = (document.getElementById("orderSearch")?.value || "").toLowerCase().trim();
    const selectStatus = document.getElementById("orderStatusSelect")?.value || "all";
    const finalStatus = activeOrderFilter !== "all" ? activeOrderFilter : selectStatus;

    const list = orders.filter(o => {
        const okSearch = !search || `${o.idOrder} ${o.cust}`.toLowerCase().includes(search);
        const okStatus = finalStatus === "all" || o.status === finalStatus;
        return okSearch && okStatus;
    });

    // إحصائيات
    setText("allOrdersStat", orders.length);
    setText("pendingOrdersStat", orders.filter(o => o.status === "pending").length);
    setText("confirmedOrdersStat", orders.filter(o => o.status === "confirmed").length);
    setText("preparingOrdersStat", orders.filter(o => o.status === "preparing").length);
    setText("shippedOrdersStat", orders.filter(o => o.status === "shipped").length);
    setText("deliveredOrdersStat", orders.filter(o => o.status === "delivered").length);
    setText("ordersCountBadge", orders.filter(o => o.status === "pending").length);

    ordersBody.innerHTML = list.map(o => `
        <tr onclick="openOrder(${o.idOrder})">
          <td><strong>#${o.idOrder}</strong></td>
          <td>
            <div style="display:flex;align-items:center;gap:.5rem">
              <div class="cust-ava">${(o.cust || "?")[0]}</div>${o.cust}
            </div>
          </td>
          <td style="color:var(--muted);font-size:.78rem">${o.date}</td>
          <td><span class="badge b-grey">${o.prods} item(s)</span></td>
          <td><strong>${money(o.total)}</strong></td>
          <td>${statusBadge(o.status)}</td>
          <td><button class="btn btn-sm btn-o" onclick="event.stopPropagation();openOrder(${o.idOrder})">Details</button></td>
        </tr>`).join("") || `<tr><td colspan="7" style="text-align:center;color:var(--muted)">No orders found.</td></tr>`;
}

/* =============================================================
   ORDERS — Detail Modal + Status Update
============================================================= */
function openOrder(idOrder) {
    const o = orders.find(x => x.idOrder === idOrder);
    const body = document.getElementById("orderModalBody");
    if (!o || !body) return;

    body.innerHTML = `
        <div class="grid-2" style="margin-bottom:1rem">
          <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem">
            <div style="font-size:.72rem;font-weight:800;color:var(--muted);margin-bottom:.6rem">Customer Information</div>
            <div style="font-size:.88rem;font-weight:700">${o.cust}</div>
            <div style="font-size:.78rem;color:var(--muted);margin-top:3px">📞 ${o.phone || "—"}</div>
            <div style="font-size:.78rem;color:var(--muted)">📍 ${o.address || "—"}</div>
          </div>
          <div style="background:var(--gu);border:1px solid var(--b);border-radius:12px;padding:1rem">
            <div style="font-size:.72rem;font-weight:800;color:var(--muted);margin-bottom:.6rem">Order Details</div>
            <div style="font-size:.78rem;color:var(--muted)">📅 ${o.date}</div>
            <div style="font-size:.82rem;margin-top:6px"><strong>Order ID:</strong> #${o.idOrder}</div>
            <div style="font-size:.82rem;margin-top:6px"><strong>Total:</strong> ${money(o.total)}</div>
            <div style="font-size:.82rem;margin-top:6px"><strong>Status:</strong> ${statusBadge(o.status)}</div>
          </div>
        </div>
        <div class="card" style="box-shadow:none">
          <div class="card-hd"><span class="card-title">🛒 Ordered Items</span></div>
          <div class="card-body">
            ${(o.items || []).map(item => `<div style="padding:.55rem 0;border-bottom:1px solid var(--b)">${item}</div>`).join("") || "<div style='color:var(--muted)'>No items.</div>"}
          </div>
        </div>`;

    // أزرار تغيير الـ Status
    const actionsDiv = document.getElementById("orderStatusActions");
    if (actionsDiv) {
        const nextStatuses = orderStatusFlow.filter(s => s !== o.status && s !== "cancelled");
        actionsDiv.innerHTML = nextStatuses.map(s => {
            const st = statusMap[s];
            return `<button class="btn btn-sm btn-o" onclick="updateOrderStatus(${o.idOrder},'${s}')">
                        → ${st ? st.lbl : s}
                    </button>`;
        }).join("") + (o.status !== "cancelled"
            ? `<button class="btn btn-sm btn-red" onclick="updateOrderStatus(${o.idOrder},'cancelled')">✕ Cancel</button>`
            : "");
    }

    openModal("orderModal");
}

/** تغيير حالة الطلب وإرسال للسيرفر */
async function updateOrderStatus(idOrder, status) {
    try {
        const formData = new FormData();
        formData.append("idOrder", idOrder);
        formData.append("status", status);

        const res = await fetch("/StoreAdmin/UpdateOrderStatus", { method: "POST", body: formData });
        const data = await res.json();

        if (!data.success) {
            alert(data.message || "Failed to update status.");
            return;
        }

        // تحديث الـ local array
        const o = orders.find(x => x.idOrder === idOrder);
        if (o) o.status = status;

        closeModal("orderModal");
        renderOrders();
        updateDashboardStats();
        showToast(`✅ Order marked as ${statusMap[status]?.lbl || status}`);

    } catch (err) {
        console.error(err);
        alert("Network error.");
    }
}

/* =============================================================
   CUSTOMERS
============================================================= */
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
              <div class="cust-ava">${(c.name || "?")[0]}</div>
              <div><div style="font-weight:700">${c.name}</div></div>
            </div>
          </td>
          <td style="color:var(--muted);font-size:.8rem">${c.email}</td>
          <td style="font-size:.82rem">${c.phone}</td>
          <td><span class="badge b-blue">${c.orders} orders</span></td>
          <td><strong style="color:var(--gd)">${money(c.spent)}</strong></td>
          <td style="color:var(--muted);font-size:.78rem">${c.last}</td>
        </tr>`).join("") || `<tr><td colspan="6" style="text-align:center;color:var(--muted)">No customers found.</td></tr>`;
}

/* =============================================================
   COMMON UI
============================================================= */
function openModal(id) { document.getElementById(id)?.classList.add("on"); }
function closeModal(id) { document.getElementById(id)?.classList.remove("on"); }

document.addEventListener("click", e => {
    if (e.target.classList.contains("overlay")) e.target.classList.remove("on");
});

function switchTab(el, paneId) {
    const card = el.closest(".card, .table-card, .products-card");
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

function showToastMsg(msg) { showToast(msg); }

/* =============================================================
   CHART (Dashboard)
============================================================= */
function initChart() {
    const ctx = document.getElementById("salesChart");
    if (!ctx || typeof Chart === "undefined") return;
    new Chart(ctx, {
        type: "line",
        data: {
            labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            datasets: [{
                label: "Sales (JOD)",
                data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],   // ← placeholder, يُستبدل ببيانات حقيقية لاحقاً
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
                tooltip: { callbacks: { label: ctx => `${ctx.parsed.y} JOD` } }
            },
            scales: {
                y: { grid: { color: "rgba(90,143,56,.07)" }, ticks: { color: "#8a9e72" } },
                x: { grid: { display: false }, ticks: { color: "#8a9e72" } }
            }
        }
    });
}

/* =============================================================
   BIND EVENTS + DOMContentLoaded
============================================================= */
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
    if (document.getElementById("salesChart")) initChart();
});