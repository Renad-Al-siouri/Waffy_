/* ============================================================
   AdminStore.js — Waffy Store Admin — Clean Unified Version
   ============================================================ */
'use strict';

// ════════════════════════════════════════════════════════════════
// STATE
// ════════════════════════════════════════════════════════════════
let orders = window.serverOrders || [];
let customers = window.serverCustomers || [];

// ════════════════════════════════════════════════════════════════
// SHARED HELPERS
// ════════════════════════════════════════════════════════════════
function money(v) { return `${Number(v || 0).toFixed(3)} JD`; }
function setText(id, val) { const el = document.getElementById(id); if (el) el.textContent = val; }

function showToast(msg = '✅ Done!') {
    const t = document.getElementById('toast');
    if (!t) return;
    t.textContent = msg;
    t.style.display = 'flex';
    setTimeout(() => { t.style.display = 'none'; }, 2400);
}
function openModal(id) { document.getElementById(id)?.classList.add('on'); }
function closeModal(id) { document.getElementById(id)?.classList.remove('on'); }

function switchTab(el, paneId) {
    const card = el.closest('.card, .table-card, .products-card');
    if (!card) return;
    card.querySelectorAll('.tab').forEach(t => t.classList.remove('on'));
    card.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('on'));
    el.classList.add('on');
    document.getElementById(paneId)?.classList.add('on');
}

// Close overlay modal on backdrop click
document.addEventListener('click', e => {
    if (e.target.classList.contains('overlay')) e.target.classList.remove('on');
});

// Safe cross-browser date parser for "dd MMM yyyy"
function _parseDate(str) {
    if (!str) return null;
    const MONTHS = { Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5, Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11 };
    const parts = str.split(' ');
    if (parts.length === 3) return new Date(+parts[2], MONTHS[parts[1]] ?? 0, +parts[0]);
    const d = new Date(str);
    return isNaN(d) ? null : d;
}

// ════════════════════════════════════════════════════════════════
// STATUS CONFIGURATION
// ════════════════════════════════════════════════════════════════
const STATUS_MAP = {
    pending: { lbl: 'Pending', cls: 'b-ora' },
    confirmed: { lbl: 'Confirmed', cls: 'b-blue' },
    preparing: { lbl: 'Preparing', cls: 'b-purple' },
    shipped: { lbl: 'Shipped', cls: 'b-teal' },
    delivered: { lbl: 'Delivered', cls: 'b-green' },
    cancelled: { lbl: 'Cancelled', cls: 'b-red' },
};

// Which statuses can follow each status
const STATUS_NEXT = {
    pending: ['confirmed', 'cancelled'],
    confirmed: ['preparing', 'cancelled'],
    preparing: ['shipped', 'cancelled'],
    shipped: ['delivered', 'cancelled'],
    delivered: [],
    cancelled: []
};

function statusBadge(s) {
    const st = STATUS_MAP[s] || { lbl: s, cls: 'b-grey' };
    return `<span class="badge ${st.cls}">${st.lbl}</span>`;
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — KPI STATS
// ════════════════════════════════════════════════════════════════
function updateDashboardStats() {
    const st = window.serverStats || {};
    setText('salesKpi', Number(st.revenue || 0).toFixed(2));
    setText('ordersKpi', st.total || 0);
    setText('newOrdersKpi', st.pending || 0);
    setText('completedKpi', st.completed || 0);
    setText('productsKpi', st.products || 0);
    setText('productsCountBadge', st.products || 0);
    setText('ordersCountBadge', st.pending || 0);

    const lsc = document.getElementById('lowStockChip');
    if (lsc) {
        lsc.textContent = `${st.lowStock || 0} low stock`;
        lsc.style.display = (st.lowStock || 0) === 0 ? 'none' : '';
    }
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — RECENT ORDERS TABLE
// ════════════════════════════════════════════════════════════════
function renderRecentOrders() {
    const body = document.getElementById('recentOrdersBody');
    if (!body) return;
    const list = orders.slice(0, 8);
    if (!list.length) {
        body.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-muted);padding:24px">No orders yet.</td></tr>';
        return;
    }
    body.innerHTML = list.map(o => `
        <tr>
            <td><span style="font-weight:900;font-family:monospace">#${o.idOrder || o.id}</span></td>
            <td>${o.cust || '—'}</td>
            <td><span style="font-size:.8rem">${o.prods || 0} item(s)</span></td>
            <td><strong>${money(o.total)}</strong></td>
            <td>${statusBadge(o.status || 'pending')}</td>
            <td>
                <button class="btn btn-outline btn-sm"
                    onclick="openOrder(${o.idOrder || o.id})">View</button>
            </td>
        </tr>`).join('');
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — BEST SELLERS
// ════════════════════════════════════════════════════════════════
function renderBestSellers() {
    const el = document.getElementById('bestSellersList');
    if (!el) return;

    // Aggregate from order items (items are now objects)
    const counts = {};
    orders.forEach(o => {
        (o.items || []).forEach(item => {
            const name = typeof item === 'object' ? item.name : (item.split(' × ')[0]);
            const qty = typeof item === 'object' ? item.qty : Number((item.split(' × ')[1]) || 1);
            const img = typeof item === 'object' ? (item.image || '') : '';
            if (!counts[name]) counts[name] = { qty: 0, img };
            counts[name].qty += qty;
        });
    });

    const sorted = Object.entries(counts).sort((a, b) => b[1].qty - a[1].qty).slice(0, 5);

    if (sorted.length) {
        el.innerHTML = sorted.map(([name, data], i) => `
            <div style="display:flex;align-items:center;gap:10px;padding:10px 0;border-bottom:1px solid var(--border)">
                <span style="font-size:1rem;font-weight:900;color:var(--brand);min-width:22px">#${i + 1}</span>
                ${data.img
                ? `<img src="${data.img}" style="width:36px;height:36px;object-fit:cover;border-radius:7px">`
                : `<div style="width:36px;height:36px;border-radius:7px;background:var(--brand-ghost);display:flex;align-items:center;justify-content:center;font-size:.9rem">📦</div>`}
                <div style="flex:1;min-width:0">
                    <div style="font-weight:800;font-size:.82rem;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${name}</div>
                    <div style="font-size:.72rem;color:var(--text-muted)">${data.qty} sold</div>
                </div>
            </div>`).join('');
        return;
    }

    // Fallback: show first 5 products from serverProducts
    const prods = (window.serverProducts || []).slice(0, 5);
    if (!prods.length) {
        el.innerHTML = '<p style="color:var(--text-muted);font-size:.84rem;text-align:center;padding:20px">No products yet.</p>';
        return;
    }
    el.innerHTML = prods.map((p, i) => `
        <div style="display:flex;align-items:center;gap:10px;padding:10px 0;border-bottom:1px solid var(--border)">
            <span style="font-size:1rem;font-weight:900;color:var(--brand);min-width:22px">#${i + 1}</span>
            ${p.image
            ? `<img src="${p.image}" style="width:36px;height:36px;object-fit:cover;border-radius:7px">`
            : `<div style="width:36px;height:36px;border-radius:7px;background:var(--brand-ghost);display:flex;align-items:center;justify-content:center;font-size:.9rem">📦</div>`}
            <div style="flex:1;min-width:0">
                <div style="font-weight:800;font-size:.82rem;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${p.name}</div>
                <div style="font-size:.72rem;color:var(--text-muted)">${p.price?.toFixed ? p.price.toFixed(3) : p.price} JD</div>
            </div>
        </div>`).join('');
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — LOW STOCK ALERT
// ════════════════════════════════════════════════════════════════
function renderLowStock() {
    const products = (window.serverProducts || []).filter(p => p.stock <= 2);
    const card = document.getElementById('lowStockCard');
    if (!products.length) { if (card) card.style.display = 'none'; return; }
    if (card) card.style.display = '';
    const tbody = document.getElementById('lowStockBody');
    if (!tbody) return;
    tbody.innerHTML = products.map(p => `
        <tr>
            <td>
                <div style="display:flex;align-items:center;gap:8px">
                    ${p.image
            ? `<img src="${p.image}" style="width:36px;height:36px;object-fit:cover;border-radius:6px">`
            : `<div style="width:36px;height:36px;border-radius:6px;background:var(--brand-ghost);display:flex;align-items:center;justify-content:center">📦</div>`}
                    <span style="font-weight:700">${p.name}</span>
                </div>
            </td>
            <td><span style="color:var(--danger);font-weight:900">${p.stock}</span></td>
            <td>${p.stock === 0
            ? '<span style="color:#e74c3c;font-weight:700;font-size:.78rem">Out of Stock</span>'
            : '<span style="color:#f39c12;font-weight:700;font-size:.78rem">Low Stock</span>'}</td>
        </tr>`).join('');
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — SALES CHART
// ════════════════════════════════════════════════════════════════
function initChart() {
    const ctx = document.getElementById('salesChart');
    if (!ctx || typeof Chart === 'undefined') return;
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const data = new Array(12).fill(0);
    orders.forEach(o => {
        const d = _parseDate(o.date);
        if (d) data[d.getMonth()] += parseFloat(o.total || 0);
    });
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: months,
            datasets: [{
                label: 'Revenue (JD)',
                data,
                backgroundColor: 'rgba(139,169,74,.25)',
                borderColor: 'rgb(139,169,74)',
                borderWidth: 2,
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,.05)' } },
                x: { grid: { display: false } }
            }
        }
    });
}

// ════════════════════════════════════════════════════════════════
// ORDERS — STATS COUNTERS
// ════════════════════════════════════════════════════════════════
function _updateOrderStats() {
    const c = {
        all: orders.length,
        pending: orders.filter(o => o.status === 'pending').length,
        confirmed: orders.filter(o => o.status === 'confirmed').length,
        preparing: orders.filter(o => o.status === 'preparing').length,
        shipped: orders.filter(o => o.status === 'shipped').length,
        delivered: orders.filter(o => o.status === 'delivered').length,
        cancelled: orders.filter(o => o.status === 'cancelled').length,
    };
    setText('statAll', c.all);
    setText('statPending', c.pending);
    setText('statConfirmed', c.confirmed);
    setText('statPreparing', c.preparing);
    setText('statShipped', c.shipped);
    setText('statDelivered', c.delivered);
    setText('ordersCountBadge', c.pending);
}

// ════════════════════════════════════════════════════════════════
// ORDERS — FILTER & RENDER TABLE
// ════════════════════════════════════════════════════════════════
let _orderFilter = 'all';

function filterOrders(status) {
    _orderFilter = status;
    const sel = document.getElementById('orderStatusSelect');
    if (sel) sel.value = status;
    renderOrders();
}

function renderOrders() {
    const body = document.getElementById('ordersBody');
    if (!body) return;

    const q = (document.getElementById('orderSearch')?.value || '').toLowerCase();
    const sel = document.getElementById('orderStatusSelect')?.value || 'all';
    const fin = _orderFilter !== 'all' ? _orderFilter : sel;

    let list = orders;
    if (fin !== 'all') list = list.filter(o => o.status === fin);
    if (q) list = list.filter(o =>
        `${o.idOrder || o.id} ${o.cust || ''}`.toLowerCase().includes(q));

    _updateOrderStats();

    body.innerHTML = list.map(o => `
        <tr>
            <td><span style="font-weight:900;font-family:monospace">#${o.idOrder || o.id}</span></td>
            <td>
                <div style="font-weight:700">${o.cust || '—'}</div>
                <div style="font-size:.72rem;color:var(--text-muted)">${o.phone || ''}</div>
            </td>
            <td style="font-size:.8rem;color:var(--text-muted)">${o.date || '—'}</td>
            <td style="font-size:.82rem">${o.prods || 0} item(s)</td>
            <td><strong>${money(o.total)}</strong></td>
            <td>${statusBadge(o.status || 'pending')}</td>
            <td>
                <button class="btn btn-outline btn-sm"
                    onclick="openOrder(${o.idOrder || o.id})">View</button>
            </td>
        </tr>`).join('') ||
        '<tr><td colspan="7" style="text-align:center;color:var(--text-muted);padding:24px">No orders found.</td></tr>';
}

// ════════════════════════════════════════════════════════════════
// ORDER DETAIL MODAL — BUILD HELPERS
// ════════════════════════════════════════════════════════════════

/** Renders the ordered items list with product images */
function _buildItemsHtml(items) {
    if (!items || !items.length) return '<li style="color:var(--text-muted);list-style:none">—</li>';
    return items.map(item => {
        // New format: object with name, qty, price, image
        if (typeof item === 'object') {
            const lineTotal = (item.price || 0) * (item.qty || 1);
            return `
                <li style="display:flex;align-items:center;gap:12px;margin-bottom:12px;
                           list-style:none;padding-bottom:10px;border-bottom:1px solid var(--border)">
                    ${item.image
                    ? `<img src="${item.image}"
                               style="width:52px;height:52px;object-fit:cover;border-radius:10px;
                                      border:1px solid var(--border);flex-shrink:0">`
                    : `<div style="width:52px;height:52px;border-radius:10px;background:var(--brand-ghost);
                                       display:flex;align-items:center;justify-content:center;
                                       flex-shrink:0;font-size:1.4rem">📦</div>`}
                    <div style="flex:1;min-width:0">
                        <div style="font-weight:700;font-size:.86rem">${item.name}</div>
                        <div style="font-size:.74rem;color:var(--text-muted);margin-top:2px">
                            × ${item.qty} &nbsp;·&nbsp; ${money(item.price)} each
                        </div>
                    </div>
                    <div style="font-weight:900;font-size:.88rem;color:var(--brand-dark);white-space:nowrap">
                        ${money(lineTotal)}
                    </div>
                </li>`;
        }
        // Fallback: legacy string format "Product × qty"
        return `<li style="font-size:.84rem;margin-bottom:6px">${item}</li>`;
    }).join('');
}

/** Shows delivery-person info when order is shipped/delivered */
function _buildDeliveryPersonHtml(o) {
    if (!['shipped', 'delivered'].includes(o.status)) return '';
    const hasInfo = o.deliveryPersonName || o.deliveryPersonPhone;
    return `
        <div style="background:var(--brand-ghost);border:1px solid var(--brand-light);
                    border-radius:12px;padding:14px;margin:0 16px 16px">
            <div style="font-size:.74rem;font-weight:800;color:var(--brand-dark);margin-bottom:8px">
                🚚 Delivery Person
            </div>
            ${hasInfo
            ? `<div style="font-weight:700">${o.deliveryPersonName || '—'}</div>
                   <div style="font-size:.8rem;color:var(--text-muted)">
                       📞 ${o.deliveryPersonPhone || '—'}
                   </div>`
            : `<span style="font-size:.8rem;color:var(--text-muted)">Not assigned yet.</span>`}
        </div>`;
}

// ════════════════════════════════════════════════════════════════
// ORDER DETAIL MODAL — OPEN
// ════════════════════════════════════════════════════════════════
function openOrder(idOrder) {
    const o = orders.find(x => (x.idOrder || x.id) == idOrder);
    const body = document.getElementById('orderModalBody');
    if (!o || !body) return;

    body.innerHTML = `
        <!-- Customer & Order Info -->
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;padding:16px">
            <div style="background:var(--brand-ghost);border:1px solid var(--border);
                        border-radius:12px;padding:12px">
                <div style="font-size:.72rem;font-weight:800;color:var(--text-muted);margin-bottom:6px">Customer</div>
                <div style="font-weight:700">${o.cust || '—'}</div>
                <div style="font-size:.78rem;color:var(--text-muted)">📞 ${o.phone || '—'}</div>
                <div style="font-size:.78rem;color:var(--text-muted);margin-top:2px">📍 ${o.address || '—'}</div>
            </div>
            <div style="background:var(--brand-ghost);border:1px solid var(--border);
                        border-radius:12px;padding:12px">
                <div style="font-size:.72rem;font-weight:800;color:var(--text-muted);margin-bottom:6px">Order Info</div>
                <div style="font-weight:900;font-family:monospace">#${o.idOrder || o.id}</div>
                <div style="font-size:.78rem;color:var(--text-muted)">${o.date || '—'}</div>
                <div style="font-size:.9rem;margin-top:6px"><strong>${money(o.total)}</strong></div>
                <div style="margin-top:6px">${statusBadge(o.status || 'pending')}</div>
            </div>
        </div>

        <!-- Delivery Person (shown when shipped/delivered) -->
        ${_buildDeliveryPersonHtml(o)}

        <!-- Items List with Images -->
        <div style="padding:0 16px 4px">
            <div style="font-size:.78rem;font-weight:800;color:var(--brand-dark);margin-bottom:10px">
                Order Items:
            </div>
            <ul style="margin:0;padding:0">
                ${_buildItemsHtml(o.items)}
            </ul>
        </div>

        <!-- Total -->
        <div style="margin:0 16px 16px;padding-top:12px;border-top:1px solid var(--border);
                    display:flex;justify-content:space-between;align-items:center">
            <span style="font-weight:800">Total Amount:</span>
            <span style="font-size:1.1rem;font-weight:900;color:var(--brand-dark)">${money(o.total)}</span>
        </div>

        <!-- Delivery Person Assignment Form (hidden — shown on "Ship" click) -->
        <div id="deliveryPersonSection" style="display:none;padding:0 16px 16px;
             border-top:1px solid var(--border);margin-top:0;padding-top:16px">
            <div style="font-size:.78rem;font-weight:800;color:var(--brand-dark);margin-bottom:12px">
                🚚 Assign Delivery Person
            </div>
            <div class="form-group">
                <div class="form-field">
                    <label class="form-label">Full Name <span class="req">*</span></label>
                    <input id="dpName" class="form-input" placeholder="Delivery person's name">
                </div>
                <div class="form-field">
                    <label class="form-label">Phone Number <span class="req">*</span></label>
                    <input id="dpPhone" class="form-input" placeholder="+962 7X XXX XXXX">
                </div>
            </div>
        </div>`;

    // Build footer action buttons
    const actDiv = document.getElementById('orderStatusActions');
    if (actDiv) _buildStatusButtons(actDiv, o);

    openModal('orderModal');
}

function _buildStatusButtons(actDiv, o) {
    const next = STATUS_NEXT[o.status] || [];
    actDiv.innerHTML = next.map(s => {
        const LABELS = {
            confirmed: '✅ Confirm',
            preparing: '🍳 Preparing',
            shipped: '🚚 Ship',
            delivered: '✔️ Delivered',
            cancelled: '❌ Cancel'
        };
        const isCancel = s === 'cancelled';
        return `<button class="btn btn-sm ${isCancel ? 'btn-danger' : 'btn-primary'}"
                    onclick="handleStatusChange(${o.idOrder || o.id},'${s}')">
                    ${LABELS[s] || s}
                </button>`;
    }).join('');
}

// ════════════════════════════════════════════════════════════════
// ORDER DELIVERY PERSON FLOW
// ════════════════════════════════════════════════════════════════

/** Called when any status-action button is clicked */
function handleStatusChange(idOrder, newStatus) {
    if (newStatus === 'shipped') {
        // Reveal delivery person form
        const section = document.getElementById('deliveryPersonSection');
        if (section) section.style.display = '';

        // Replace footer buttons
        const actDiv = document.getElementById('orderStatusActions');
        if (actDiv) {
            actDiv.innerHTML = `
                <button class="btn btn-outline btn-sm"
                    onclick="cancelDeliveryForm(${idOrder})">← Back</button>
                <button class="btn btn-primary btn-sm"
                    onclick="confirmShipment(${idOrder})">🚚 Confirm Shipment</button>`;
        }
    } else {
        updateOrderStatus(idOrder, newStatus);
    }
}

/** Hides the delivery form and resets to the normal detail view */
function cancelDeliveryForm(idOrder) {
    openOrder(idOrder); // Rebuilds the modal from scratch
}

/** Saves delivery person and marks order as Shipped */
async function confirmShipment(idOrder) {
    const name = (document.getElementById('dpName')?.value || '').trim();
    const phone = (document.getElementById('dpPhone')?.value || '').trim();
    if (!name || !phone) {
        showToast('⚠️ Please enter delivery person name and phone.');
        return;
    }

    const fd = new FormData();
    fd.append('idOrder', idOrder);
    fd.append('deliveryPersonName', name);
    fd.append('deliveryPersonPhone', phone);

    try {
        const res = await fetch('/StoreAdmin/UpdateDeliveryPerson', { method: 'POST', body: fd });
        const data = await res.json();
        if (data.success) {
            // Update local state
            const o = orders.find(x => (x.idOrder || x.id) == idOrder);
            if (o) {
                o.status = 'shipped';
                o.deliveryPersonName = name;
                o.deliveryPersonPhone = phone;
            }
            closeModal('orderModal');
            renderOrders();
            _updateOrderStats();
            showToast('✅ Order shipped — delivery person assigned!');
        } else {
            showToast('❌ ' + (data.message || 'Error saving shipment.'));
        }
    } catch (e) {
        showToast('❌ Network error.');
        console.error(e);
    }
}

/** Standard status update (non-Shipped transitions) */
async function updateOrderStatus(idOrder, status) {
    const fd = new FormData();
    fd.append('idOrder', idOrder);
    fd.append('status', status);
    try {
        const res = await fetch('/StoreAdmin/UpdateOrderStatus', { method: 'POST', body: fd });
        const data = await res.json();
        if (data.success) {
            const o = orders.find(x => (x.idOrder || x.id) == idOrder);
            if (o) o.status = status;
            closeModal('orderModal');
            renderOrders();
            _updateOrderStats();
            showToast(`✅ Order → ${STATUS_MAP[status]?.lbl || status}`);
        } else {
            showToast('❌ ' + (data.message || 'Update failed.'));
        }
    } catch (e) {
        showToast('❌ Network error.');
        console.error(e);
    }
}

// ════════════════════════════════════════════════════════════════
// ORDERS — CSV EXPORT
// ════════════════════════════════════════════════════════════════
function exportOrders() {
    if (!orders.length) { showToast('⚠️ No orders to export.'); return; }
    const rows = ['Order ID,Customer,Phone,Date,Items,Total,Status,Delivery Person,Delivery Phone'];
    orders.forEach(o => rows.push(
        `"${o.idOrder || o.id}","${o.cust || ''}","${o.phone || ''}","${o.date || ''}",` +
        `"${o.prods || 0}","${money(o.total)}","${o.status || ''}",` +
        `"${o.deliveryPersonName || ''}","${o.deliveryPersonPhone || ''}"`
    ));
    const blob = new Blob([rows.join('\n')], { type: 'text/csv' });
    Object.assign(document.createElement('a'), {
        href: URL.createObjectURL(blob),
        download: 'orders.csv'
    }).click();
}

// ════════════════════════════════════════════════════════════════
// CUSTOMERS — RENDER TABLE
// ════════════════════════════════════════════════════════════════
function renderCustomers() {
    const body = document.getElementById('customersBody');
    if (!body) return;

    const q = (document.getElementById('customerSearch')?.value || '').toLowerCase();
    const srt = document.getElementById('customerSort')?.value || 'spending';
    let list = customers.filter(c =>
        !q || `${c.name || ''} ${c.email || ''}`.toLowerCase().includes(q));

    if (srt === 'spending') list.sort((a, b) => b.spent - a.spent);
    else if (srt === 'newest') list.sort((a, b) => b.orders - a.orders);
    else if (srt === 'oldest') list.sort((a, b) => a.orders - b.orders);

    body.innerHTML = list.map(c => `
        <tr>
            <td>
                <div style="display:flex;align-items:center;gap:.6rem">
                    <div class="cust-ava">${(c.name || '?')[0]}</div>
                    <div style="font-weight:700">${c.name || '—'}</div>
                </div>
            </td>
            <td style="color:var(--text-muted);font-size:.8rem">${c.email || '—'}</td>
            <td style="font-size:.82rem">${c.phone || '—'}</td>
            <td><span class="badge b-blue">${c.orders} orders</span></td>
            <td><strong style="color:var(--success)">${money(c.spent)}</strong></td>
            <td style="color:var(--text-muted);font-size:.78rem">${c.last || '—'}</td>
        </tr>`).join('') ||
        '<tr><td colspan="6" style="text-align:center;color:var(--text-muted);padding:24px">No customers yet.</td></tr>';
}

// ════════════════════════════════════════════════════════════════
// COMPAT SHIM — called from some legacy onclick handlers
// ════════════════════════════════════════════════════════════════
function addProduct() {
    if (typeof dashAddProduct === 'function') dashAddProduct();
    else if (typeof submitAddProduct === 'function') submitAddProduct();
}

// ════════════════════════════════════════════════════════════════
// AUTO-INIT ON DOMContentLoaded
// ════════════════════════════════════════════════════════════════
document.addEventListener('DOMContentLoaded', () => {

    // Orders page event listeners
    document.getElementById('orderSearch')?.addEventListener('input', () => { _orderFilter = 'all'; renderOrders(); });
    document.getElementById('orderStatusSelect')?.addEventListener('change', () => { _orderFilter = 'all'; renderOrders(); });

    // Customers page event listeners
    document.getElementById('customerSearch')?.addEventListener('input', renderCustomers);
    document.getElementById('customerSort')?.addEventListener('change', renderCustomers);

    // Run functions based on which page elements exist
    if (document.getElementById('recentOrdersBody')) renderRecentOrders();
    if (document.getElementById('bestSellersList')) renderBestSellers();
    if (document.getElementById('lowStockBody')) renderLowStock();
    if (document.getElementById('ordersBody')) renderOrders();
    if (document.getElementById('customersBody')) renderCustomers();
    if (document.getElementById('salesChart')) initChart();
    if (document.getElementById('salesKpi')) updateDashboardStats();
});