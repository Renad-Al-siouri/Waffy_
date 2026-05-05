/* ============================================================
   AdminStore.js — Waffy Store Admin — النسخة النهائية الكاملة
   ============================================================ */

'use strict';

// ════════════════════════════════════════════════════════════════
// STATE — تُملأ من window.server* في كل صفحة
// ════════════════════════════════════════════════════════════════
let orders = window.serverOrders || [];
let customers = window.serverCustomers || [];

// ════════════════════════════════════════════════════════════════
// HELPERS المشتركة
// ════════════════════════════════════════════════════════════════
function money(v) { return `${Number(v || 0).toFixed(3)} JD`; }

function setText(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
}

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

// إغلاق modal عند النقر على الـ overlay
document.addEventListener('click', e => {
    if (e.target.classList.contains('overlay')) e.target.classList.remove('on');
});

// ════════════════════════════════════════════════════════════════
// STATUS
// ════════════════════════════════════════════════════════════════
const statusMap = {
    pending: { lbl: 'Pending', cls: 'b-ora' },
    confirmed: { lbl: 'Confirmed', cls: 'b-blue' },
    preparing: { lbl: 'Preparing', cls: 'b-purple' },
    shipped: { lbl: 'Shipped', cls: 'b-teal' },
    delivered: { lbl: 'Delivered', cls: 'b-green' },
    cancelled: { lbl: 'Cancelled', cls: 'b-red' },
    active: { lbl: 'Active', cls: 'b-green' },
    out: { lbl: 'Out of Stock', cls: 'b-red' },
    hidden: { lbl: 'Hidden', cls: 'b-grey' }
};
const orderStatusFlow = ['pending', 'confirmed', 'preparing', 'shipped', 'delivered', 'cancelled'];

function statusBadge(status) {
    const st = statusMap[status] || { lbl: status, cls: 'b-grey' };
    return `<span class="badge ${st.cls}">${st.lbl}</span>`;
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD STATS
// ════════════════════════════════════════════════════════════════
function updateDashboardStats() {
    const stats = window.serverStats || {};
    const rev = stats.revenue != null ? Number(stats.revenue).toFixed(2) : '0.00';
    setText('salesKpi', rev);
    setText('ordersKpi', stats.total || 0);
    setText('newOrdersKpi', stats.pending || 0);
    setText('completedKpi', stats.completed || 0);
    setText('completedOrdersKpi', stats.completed || 0);
    setText('productsKpi', stats.products || 0);
    setText('productsCountBadge', stats.products || 0);
    setText('ordersCountBadge', stats.pending || 0);
    setText('stockTrendText', `${stats.lowStock || 0} low stock`);
    setText('lowStockChip', `${stats.lowStock || 0} low stock`);
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — Today Orders
// ════════════════════════════════════════════════════════════════
function renderTodayOrders() {
    const body = document.getElementById('todayOrdersBody') ||
        document.getElementById('recentOrdersBody');
    if (!body) return;
    body.innerHTML = orders.slice(0, 8).map(o => `
        <tr onclick="openOrder(${o.idOrder || o.id})" style="cursor:pointer">
            <td><strong>#${o.idOrder || o.id}</strong></td>
            <td>
                <div style="display:flex;align-items:center;gap:.5rem">
                    <div class="cust-ava">${(o.cust || '?')[0]}</div>
                    ${o.cust || '—'}
                </div>
            </td>
            <td><span class="badge b-grey">${o.prods || 0} item(s)</span></td>
            <td><strong>${money(o.total || 0)}</strong></td>
            <td>${statusBadge(o.status || 'pending')}</td>
            <td>
                <button class="btn btn-sm btn-o"
                    onclick="event.stopPropagation();openOrder(${o.idOrder || o.id})">
                    Details
                </button>
            </td>
        </tr>`).join('') ||
        '<tr><td colspan="6" style="text-align:center;color:var(--text-muted);padding:24px">No orders yet.</td></tr>';
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — Best Sellers
// ════════════════════════════════════════════════════════════════
function renderBestSellers() {
    const el = document.getElementById('bestSellersList');
    if (!el) return;
    const counts = {};
    orders.forEach(o => (o.items || []).forEach(item => {
        const parts = item.split(' × ');
        const name = parts[0];
        const qty = Number(parts[1] || 1);
        counts[name] = (counts[name] || 0) + qty;
    }));
    const sorted = Object.entries(counts).sort((a, b) => b[1] - a[1]).slice(0, 5);
    el.innerHTML = sorted.length
        ? sorted.map(([name, qty], i) => `
            <div style="display:flex;align-items:center;gap:10px;padding:10px 0;
                        border-bottom:1px solid var(--border)">
                <span style="font-size:1rem;font-weight:900;color:var(--brand);min-width:22px">
                    #${i + 1}
                </span>
                <div style="flex:1;min-width:0">
                    <div style="font-weight:800;font-size:.82rem;
                                white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                        ${name}
                    </div>
                    <div style="font-size:.72rem;color:var(--text-muted)">${qty} sold</div>
                </div>
            </div>`).join('')
        : '<p style="color:var(--text-muted);font-size:.82rem;padding:10px 0">No sales data yet.</p>';
}

// ════════════════════════════════════════════════════════════════
// DASHBOARD — Chart
// ════════════════════════════════════════════════════════════════
function initChart() {
    const ctx = document.getElementById('salesChart');
    if (!ctx || typeof Chart === 'undefined') return;
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const data = new Array(12).fill(0);
    orders.forEach(o => {
        if (o.date) {
            const d = new Date(o.date);
            if (!isNaN(d)) data[d.getMonth()] += parseFloat(o.total || 0);
        }
    });
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: months,
            datasets: [{
                label: 'Revenue (JD)',
                data,
                backgroundColor: 'rgba(139,169,74,.22)',
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
// ORDERS — Render
// ════════════════════════════════════════════════════════════════
let activeOrderFilter = 'all';

function filterOrders(status) {
    activeOrderFilter = status;
    const sel = document.getElementById('orderStatusSelect');
    if (sel) sel.value = status === 'all' ? 'all' : status;
    renderOrders();
}

function renderOrders() {
    const body = document.getElementById('ordersBody');
    if (!body) return;

    const q = (document.getElementById('orderSearch')?.value || '').toLowerCase();
    const sel = document.getElementById('orderStatusSelect')?.value || 'all';
    const fin = activeOrderFilter !== 'all' ? activeOrderFilter : sel;

    const list = orders.filter(o => {
        const okQ = !q || `${o.idOrder || o.id} ${o.cust || ''}`.toLowerCase().includes(q);
        const okSt = fin === 'all' || o.status === fin;
        return okQ && okSt;
    });

    // تحديث الإحصائيات
    const counts = {
        all: orders.length,
        pending: orders.filter(o => o.status === 'pending').length,
        confirmed: orders.filter(o => o.status === 'confirmed').length,
        preparing: orders.filter(o => o.status === 'preparing').length,
        shipped: orders.filter(o => o.status === 'shipped').length,
        delivered: orders.filter(o => o.status === 'delivered').length
    };
    // أسماء متعددة للعناصر (للتوافق مع القديم والجديد)
    setText('statAll', counts.all);
    setText('statPending', counts.pending);
    setText('statConfirmed', counts.confirmed);
    setText('statPreparing', counts.preparing);
    setText('statShipped', counts.shipped);
    setText('statDelivered', counts.delivered);
    setText('allOrdersStat', counts.all);
    setText('pendingOrdersStat', counts.pending);
    setText('confirmedOrdersStat', counts.confirmed);
    setText('preparingOrdersStat', counts.preparing);
    setText('shippedOrdersStat', counts.shipped);
    setText('deliveredOrdersStat', counts.delivered);
    setText('ordersCountBadge', counts.pending);

    body.innerHTML = list.map(o => `
        <tr onclick="openOrder(${o.idOrder || o.id})" style="cursor:pointer">
            <td><strong>#${o.idOrder || o.id}</strong></td>
            <td>
                <div style="display:flex;align-items:center;gap:.5rem">
                    <div class="cust-ava">${(o.cust || '?')[0]}</div>
                    ${o.cust || '—'}
                </div>
            </td>
            <td style="color:var(--text-muted);font-size:.78rem">${o.date || '—'}</td>
            <td><span class="badge b-grey">${o.prods || 0} item(s)</span></td>
            <td><strong>${money(o.total || 0)}</strong></td>
            <td>${statusBadge(o.status || 'pending')}</td>
            <td>
                <button class="btn btn-sm btn-o"
                    onclick="event.stopPropagation();openOrder(${o.idOrder || o.id})">
                    Details
                </button>
            </td>
        </tr>`).join('') ||
        '<tr><td colspan="7" style="text-align:center;color:var(--text-muted);padding:24px">No orders found.</td></tr>';
}

// ════════════════════════════════════════════════════════════════
// ORDERS — Detail Modal
// ════════════════════════════════════════════════════════════════
function openOrder(idOrder) {
    const o = orders.find(x => (x.idOrder || x.id) == idOrder);
    const body = document.getElementById('orderModalBody');
    if (!o || !body) return;

    body.innerHTML = `
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;padding:16px">
            <div style="background:var(--brand-ghost);border:1px solid var(--border);
                        border-radius:12px;padding:12px">
                <div style="font-size:.72rem;font-weight:800;color:var(--text-muted);margin-bottom:6px">
                    Customer
                </div>
                <div style="font-weight:700">${o.cust || '—'}</div>
                <div style="font-size:.78rem;color:var(--text-muted)">📞 ${o.phone || '—'}</div>
                <div style="font-size:.78rem;color:var(--text-muted)">📍 ${o.address || '—'}</div>
            </div>
            <div style="background:var(--brand-ghost);border:1px solid var(--border);
                        border-radius:12px;padding:12px">
                <div style="font-size:.72rem;font-weight:800;color:var(--text-muted);margin-bottom:6px">
                    Order Info
                </div>
                <div style="font-size:.88rem;font-weight:800">#${o.idOrder || o.id}</div>
                <div style="font-size:.78rem;color:var(--text-muted)">${o.date || '—'}</div>
                <div style="font-size:.9rem;margin-top:6px">
                    <strong>${money(o.total || 0)}</strong>
                </div>
                <div style="margin-top:6px">${statusBadge(o.status || 'pending')}</div>
            </div>
        </div>
        <div style="padding:0 16px 16px">
            <div style="font-size:.78rem;font-weight:800;color:var(--brand-dark);margin-bottom:8px">
                Items:
            </div>
            <ul style="margin:0;padding-left:18px">
                ${(o.items || []).map(i =>
        `<li style="font-size:.84rem;margin-bottom:4px">${i}</li>`
    ).join('') || '<li style="color:var(--text-muted)">—</li>'}
            </ul>
        </div>`;

    const actDiv = document.getElementById('orderStatusActions');
    if (actDiv) {
        const next = orderStatusFlow.filter(s => s !== o.status && s !== 'cancelled');
        actDiv.innerHTML =
            next.map(s => `
                <button class="btn btn-sm btn-o"
                    onclick="updateOrderStatus(${o.idOrder || o.id},'${s}')">
                    → ${statusMap[s]?.lbl || s}
                </button>`).join('') +
            (o.status !== 'cancelled'
                ? `<button class="btn btn-sm btn-red"
                       onclick="updateOrderStatus(${o.idOrder || o.id},'cancelled')">
                       ✕ Cancel
                   </button>`
                : '');
    }
    openModal('orderModal');
}

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
            updateDashboardStats();
            showToast(`✅ Order → ${statusMap[status]?.lbl || status}`);
        } else {
            showToast('❌ ' + (data.message || 'Update failed'));
        }
    } catch (e) {
        showToast('❌ Network error');
        console.error(e);
    }
}

// ════════════════════════════════════════════════════════════════
// CUSTOMERS
// ════════════════════════════════════════════════════════════════
function renderCustomers() {
    const body = document.getElementById('customersBody');
    if (!body) return;

    const q = (document.getElementById('customerSearch')?.value || '').toLowerCase();
    const srt = document.getElementById('customerSort')?.value || 'spending';

    let list = customers.filter(c =>
        !q || `${c.name || ''} ${c.email || ''}`.toLowerCase().includes(q)
    );
    if (srt === 'spending') list = [...list].sort((a, b) => b.spent - a.spent);
    if (srt === 'newest') list = [...list].sort((a, b) => b.orders - a.orders);
    if (srt === 'oldest') list = [...list].sort((a, b) => a.orders - b.orders);

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
// PRODUCTS — هذه الدوال موجودة للتوافق مع Dashboard القديم فقط
// منطق Products الكامل موجود في Products.cshtml
// ════════════════════════════════════════════════════════════════

// إذا كانت صفحة Dashboard تستدعي addProduct() هذه تمنع الخطأ
function addProduct() {
    // في Dashboard_FINAL.cshtml هي dashAddProduct()
    if (typeof dashAddProduct === 'function') {
        dashAddProduct();
    } else if (typeof submitAddProduct === 'function') {
        submitAddProduct();
    }
}

// ════════════════════════════════════════════════════════════════
// BIND EVENTS + DOMContentLoaded
// ════════════════════════════════════════════════════════════════
document.addEventListener('DOMContentLoaded', () => {

    // ── Orders page ──────────────────────────────────────────────
    document.getElementById('orderSearch')?.addEventListener('input', () => {
        activeOrderFilter = 'all';
        renderOrders();
    });
    document.getElementById('orderStatusSelect')?.addEventListener('change', () => {
        activeOrderFilter = 'all';
        renderOrders();
    });

    // ── Customers page ───────────────────────────────────────────
    document.getElementById('customerSearch')?.addEventListener('input', renderCustomers);
    document.getElementById('customerSort')?.addEventListener('change', renderCustomers);

    // ── تشغيل الدوال حسب الصفحة الحالية ─────────────────────────
    if (document.getElementById('recentOrdersBody') ||
        document.getElementById('todayOrdersBody')) renderTodayOrders();

    if (document.getElementById('bestSellersList')) renderBestSellers();
    if (document.getElementById('ordersBody')) renderOrders();
    if (document.getElementById('customersBody')) renderCustomers();
    if (document.getElementById('salesChart')) initChart();
    if (document.getElementById('salesKpi')) updateDashboardStats();
});