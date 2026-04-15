using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class StoreAdminController : Controller
    {
        private readonly IStoreRepository _storeRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderItemRepository _orderItemRepo;

        public StoreAdminController(
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IOrderItemRepository orderItemRepo)
        {
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
        }

        // ── Helper: جلب المتجر من Session ──────────────────────────
        private async Task<Store?> GetCurrentStore()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;
            return await _storeRepo.GetByOwnerUserIdAsync(userId.Value);
        }

        // ── Helper: بيانات الـ Sidebar ──────────────────────────────
        private void SetSidebarData(Store store)
        {
            // ✅ الإصلاح: تحويل صريح لـ string أولاً
            string ownerName = HttpContext.Session.GetString("FullName") ?? "Store Owner";
            string initial = ownerName.Length > 0 ? ownerName[0].ToString().ToUpper() : "S";

            ViewBag.StoreName = store.Name;
            ViewBag.OwnerName = ownerName;
            ViewBag.OwnerInitial = initial;
        }

        // ── Helper: تحويل Product → JS shape ───────────────────────
        private object MapProductToJs(Product p) => new
        {
            id = p.Id,
            name = p.Name,
            cat = p.IdCategory.ToString(),
            price = p.Price,
            salePrice = p.SalePrice,
            stock = p.StockQuantity,
            maxStock = p.StockQuantity > 0 ? p.StockQuantity * 2 : 20,
            status = p.StockQuantity == 0 ? "out" : (p.IsVisible ? "active" : "hidden"),
            emoji = "📦",
            image = p.ImageUrl ?? "",
            description = p.Description ?? ""
        };

        // ── Helper: تحويل Order + Items → JS shape ─────────────────
        private async Task<object> MapOrderToJs(Order o)
        {
            var items = (await _orderItemRepo.GetByOrderIdAsync(o.IdOrder)).ToList();
            return new
            {
                id = o.OrderCode,
                idOrder = o.IdOrder,
                cust = o.RecipientName,
                date = o.CreatedAt.ToString("dd MMM yyyy"),
                prods = items.Count,
                total = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice)),
                status = (o.Status ?? "pending").ToLower(),
                items = items.Select(i => $"{i.ProductNameSnapshot} × {i.Quantity}").ToList(),
                phone = o.RecipientPhone,
                email = "",
                address = o.DeliveryAddress
            };
        }

        // ================================================================
        // INDEX
        // ================================================================
        public IActionResult Index() => RedirectToAction("Dashboard");

        // ================================================================
        // DASHBOARD — GET
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            SetSidebarData(store);

            var products = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var orders = (await _orderRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(store.IdStore);

            // تحويل للـ JS
            var productsJs = products.Select(MapProductToJs);

            var ordersJsTasks = orders.Select(MapOrderToJs);
            var ordersJs = await Task.WhenAll(ordersJsTasks);

            ViewBag.ProductsJson = JsonSerializer.Serialize(productsJs);
            ViewBag.OrdersJson = JsonSerializer.Serialize(ordersJs);
            ViewBag.TotalRevenue = revenue;
            ViewBag.PendingOrders = orders.Count(o => o.Status == "Pending");
            ViewBag.CompletedOrders = orders.Count(o => o.Status == "Delivered");
            ViewBag.LowStockCount = products.Count(p => p.StockQuantity <= 2);
            ViewBag.ProductsCount = products.Count;

            return View();
        }

        // ================================================================
        // PRODUCTS — GET
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            SetSidebarData(store);

            var products = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var productsJs = products.Select(MapProductToJs);

            ViewBag.ProductsJson = JsonSerializer.Serialize(productsJs);
            ViewBag.StoreId = store.IdStore;
            ViewBag.ProductCount = products.Count;

            return View();
        }

        // ================================================================
        // ADD PRODUCT — POST
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> AddProduct(
            string name, string? description, decimal price, decimal? salePrice,
            int stockQuantity, int idCategory, string? petType, IFormFile? imageFile)
        {
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false, message = "Not authorized" });

            string? imageUrl = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                imageUrl = $"/uploads/products/{fileName}";
            }

            var product = new Product
            {
                IdStore = store.IdStore,
                IdCategory = idCategory > 0 ? idCategory : 1,
                Name = name,
                Description = description,
                Price = price,
                SalePrice = salePrice,
                StockQuantity = stockQuantity,
                PetType = petType,
                ImageUrl = imageUrl,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _productRepo.AddAsync(product);
            return Json(new { success = true, id = newId, imageUrl });
        }

        // ================================================================
        // EDIT PRODUCT — POST
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> EditProduct(
            int id, string name, string? description, decimal price, decimal? salePrice,
            int stockQuantity, int idCategory, string? petType, IFormFile? imageFile)
        {
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false, message = "Product not found" });

            if (imageFile != null && imageFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = $"/uploads/products/{fileName}";
            }

            product.Name = name;
            product.Description = description;
            product.Price = price;
            product.SalePrice = salePrice;
            product.StockQuantity = stockQuantity;
            product.IdCategory = idCategory > 0 ? idCategory : product.IdCategory;
            product.PetType = petType;

            await _productRepo.UpdateAsync(product);
            return Json(new { success = true, imageUrl = product.ImageUrl });
        }

        // ================================================================
        // DELETE PRODUCT — POST
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false });

            await _productRepo.DeleteAsync(id);
            return Json(new { success = true });
        }

        // ================================================================
        // TOGGLE VISIBILITY — POST
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false });

            var newVisibility = !product.IsVisible;
            await _productRepo.UpdateVisibilityAsync(id, newVisibility);
            return Json(new { success = true, isVisible = newVisibility });
        }

        // ================================================================
        // ORDERS — GET
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            SetSidebarData(store);

            var orders = (await _orderRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var ordersJsTasks = orders.Select(MapOrderToJs);
            var ordersJs = await Task.WhenAll(ordersJsTasks);

            ViewBag.OrdersJson = JsonSerializer.Serialize(ordersJs);
            return View();
        }

        // ================================================================
        // UPDATE ORDER STATUS — POST
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int idOrder, string status)
        {
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var order = await _orderRepo.GetByIdAsync(idOrder);
            if (order == null || order.IdStore != store.IdStore)
                return Json(new { success = false });

            // تحويل الـ status من lowercase (JS) إلى PascalCase (DB)
            var dbStatus = status switch
            {
                "pending" => "Pending",
                "confirmed" => "Confirmed",
                "preparing" => "Preparing",
                "shipped" => "Shipped",
                "delivered" => "Delivered",
                "cancelled" => "Cancelled",
                _ => status
            };

            await _orderRepo.UpdateStatusAsync(idOrder, dbStatus);
            return Json(new { success = true });
        }

        // ================================================================
        // CUSTOMERS — GET
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            SetSidebarData(store);

            var customers = await _orderRepo.GetCustomersByStoreAsync(store.IdStore);
            var customersJs = customers.Select(c => new
            {
                name = c.FullName,
                email = c.Email ?? "",
                phone = c.Phone ?? "",
                orders = c.OrderCount,
                spent = c.TotalSpent,
                last = c.LastOrderDate?.ToString("dd MMM yyyy") ?? "-"
            });

            ViewBag.CustomersJson = JsonSerializer.Serialize(customersJs);
            return View();
        }

        // ================================================================
        // SETTINGS — GET
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            SetSidebarData(store);
            return View(store);
        }

        // ================================================================
        // SETTINGS — POST
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(
            string name, string? description, string? storeType,
            string? email, string? phone, string? whatsApp,
            string? city, string? area, string? address,
            bool deliveryAvailable, string? deliveryAreas,
            string? openTime, string? closeTime, string? workingDays,
            string? acceptedPayments,
            IFormFile? logoFile, IFormFile? coverFile)
        {
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            // رفع الشعار
            if (logoFile != null && logoFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "store-logos");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(logoFile.FileName)}";
                using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await logoFile.CopyToAsync(stream);
                store.LogoUrl = $"/uploads/store-logos/{fileName}";
            }

            // رفع صورة الغلاف
            if (coverFile != null && coverFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "store-covers");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(coverFile.FileName)}";
                using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await coverFile.CopyToAsync(stream);
                store.CoverImageUrl = $"/uploads/store-covers/{fileName}";
            }

            store.Name = name;
            store.Description = description;
            store.StoreType = storeType;
            store.Email = email;
            store.Phone = phone;
            store.WhatsApp = whatsApp;
            store.City = city;
            store.Area = area;
            store.Address = address;
            store.DeliveryAvailable = deliveryAvailable;
            store.DeliveryAreas = deliveryAreas;
            store.WorkingDays = workingDays;
            store.AcceptedPayments = acceptedPayments;

            store.OpenTime = string.IsNullOrEmpty(openTime) ? null : TimeSpan.TryParse(openTime, out var ot) ? ot : null;
            store.CloseTime = string.IsNullOrEmpty(closeTime) ? null : TimeSpan.TryParse(closeTime, out var ct) ? ct : null;

            await _storeRepo.UpdateAsync(store);
            TempData["SettingsSuccess"] = "✅ Settings saved successfully.";
            return RedirectToAction("Settings");
        }

        // ================================================================
        // صفحات لاحقة (بدون ربط بيانات الآن)
        // ================================================================
        public IActionResult Reviews() => View();
        public IActionResult Adoption() => View();
        public IActionResult Notifications() => View();
        public IActionResult Subscription() => View();
    }
}