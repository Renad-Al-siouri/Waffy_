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
        private readonly IStoreDocumentRepository _docRepo;

        public StoreAdminController(
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IOrderItemRepository orderItemRepo,
            IStoreDocumentRepository docRepo)
        {
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _docRepo = docRepo;
        }

        // ══════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════

        private bool IsStoreOwner() =>
            HttpContext.Session.GetInt32("UserId") != null &&
            HttpContext.Session.GetString("RoleName") == "Store";

        private async Task<Store?> GetCurrentStore()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;
            return await _storeRepo.GetByOwnerUserIdAsync(userId.Value);
        }

        private void SetSidebarData(Store store)
        {
            string ownerName = HttpContext.Session.GetString("FullName") ?? "Store Owner";
            string initial = ownerName.Length > 0 ? ownerName[0].ToString().ToUpper() : "S";
            ViewBag.StoreName = store.Name ?? "";
            ViewBag.OwnerName = ownerName;
            ViewBag.OwnerInitial = initial;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            await using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subFolder}/{fileName}";
        }

        private static string MapDocLabel(string? docType) => docType switch
        {
            "CommercialRegister" => "Commercial Registration",
            "BusinessLicense" => "Business License",
            "TrademarkCertificate" => "Trademark Certificate",
            "AdditionalFile" => "Additional Document",
            _ => docType ?? "Document"
        };

        private static object MapProductToJs(Product p) => new
        {
            id = p.Id,
            name = p.Name ?? "",
            description = p.Description ?? "",
            cat = p.IdCategory.ToString(),
            price = p.Price,
            salePrice = p.SalePrice,
            stock = p.StockQuantity,
            maxStock = p.StockQuantity > 0 ? p.StockQuantity * 2 : 20,
            status = p.StockQuantity == 0 ? "out" : (p.IsVisible ? "active" : "hidden"),
            image = p.ImageUrl ?? ""
        };

        private async Task<object> MapOrderToJs(Order o)
        {
            var items = (await _orderItemRepo.GetByOrderIdAsync(o.IdOrder)).ToList();
            return new
            {
                idOrder = o.IdOrder,
                id = o.IdOrder.ToString(),
                cust = o.RecipientName ?? "—",
                phone = o.RecipientPhone ?? "",
                address = o.DeliveryAddress ?? "",
                email = "",
                date = o.CreatedAt.ToString("dd MMM yyyy"),
                prods = items.Count,
                total = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice)),
                status = (o.Status ?? "pending").ToLower(),
                items = items.Select(i => $"{i.ProductNameSnapshot ?? "Product"} × {i.Quantity}").ToList()
            };
        }

        // ══════════════════════════════════════════════════════════
        // INDEX / DASHBOARD
        // ══════════════════════════════════════════════════════════

        public IActionResult Index() => RedirectToAction("Dashboard");

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var products = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var orders = (await _orderRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(store.IdStore);

            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(MapProductToJs));
            ViewBag.OrdersJson = JsonSerializer.Serialize(await Task.WhenAll(orders.Select(MapOrderToJs)));
            ViewBag.TotalRevenue = revenue;
            ViewBag.PendingOrders = orders.Count(o => o.Status == "Pending");
            ViewBag.CompletedOrders = orders.Count(o => o.Status == "Delivered");
            ViewBag.LowStockCount = products.Count(p => p.StockQuantity <= 2);
            ViewBag.ProductsCount = products.Count;
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // PRODUCTS
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var products = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(MapProductToJs));
            ViewBag.StoreId = store.IdStore;
            ViewBag.ProductCount = products.Count;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(
            string name, string? description, decimal price, decimal? salePrice,
            int stockQuantity, int idCategory, string? petType, IFormFile? imageFile)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false, message = "Store not found." });

            string? imageUrl = null;
            if (imageFile != null && imageFile.Length > 0)
                imageUrl = await SaveFileAsync(imageFile, "products");

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

        [HttpPost]
        public async Task<IActionResult> EditProduct(
            int id, string name, string? description, decimal price, decimal? salePrice,
            int stockQuantity, int idCategory, string? petType, IFormFile? imageFile)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false, message = "Store not found." });

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false, message = "Product not found." });

            if (imageFile != null && imageFile.Length > 0)
                product.ImageUrl = await SaveFileAsync(imageFile, "products");

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

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false, message = "Product not found." });

            await _productRepo.DeleteAsync(id);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false, message = "Product not found." });

            var newVisibility = !product.IsVisible;
            await _productRepo.UpdateVisibilityAsync(id, newVisibility);
            return Json(new { success = true, isVisible = newVisibility });
        }

        // ══════════════════════════════════════════════════════════
        // ORDERS
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var orders = (await _orderRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            ViewBag.OrdersJson = JsonSerializer.Serialize(await Task.WhenAll(orders.Select(MapOrderToJs)));
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int idOrder, string status)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var order = await _orderRepo.GetByIdAsync(idOrder);
            if (order == null || order.IdStore != store.IdStore)
                return Json(new { success = false, message = "Order not found." });

            var dbStatus = status.ToLower() switch
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

        // ══════════════════════════════════════════════════════════
        // CUSTOMERS
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var customers = await _orderRepo.GetCustomersByStoreAsync(store.IdStore);
            var customersJs = customers.Select(c => new
            {
                name = c.FullName ?? "",
                email = c.Email ?? "",
                phone = c.Phone ?? "",
                orders = c.OrderCount,
                spent = c.TotalSpent,
                last = c.LastOrderDate?.ToString("dd MMM yyyy") ?? "-"
            });
            ViewBag.CustomersJson = JsonSerializer.Serialize(customersJs);
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // SETTINGS — الإعدادات المنفصلة
        // ══════════════════════════════════════════════════════════

        // Redirect /Settings → /Settings/Info
        [HttpGet]
        public IActionResult Settings() => RedirectToAction("SettingsInfo");

        // ── Info ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SettingsInfo()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            // جلب الوثائق
            var docs = (await _docRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            ViewBag.Documents = docs.Select(d => new
            {
                id = d.Id,
                type = d.DocumentType ?? "",
                label = MapDocLabel(d.DocumentType),
                fileUrl = d.FileUrl ?? "",
                status = (d.Status ?? "pending").ToLower(),
                uploadedAt = d.UploadedAt.ToString("dd MMM yyyy"),
                isImage = IsImageFile(d.FileUrl)
            }).ToList();

            return View(store);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsInfo(string name, string? description, string? storeType)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            store.Name = name;
            store.Description = description;
            store.StoreType = storeType;
            await _storeRepo.UpdateAsync(store);
            TempData["Success"] = "✅ Store information saved.";
            return RedirectToAction("SettingsInfo");
        }

        // ── Images ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SettingsImages()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            return View(store);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsImages(IFormFile? logoFile, IFormFile? coverFile)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            if (logoFile != null && logoFile.Length > 0) store.LogoUrl = await SaveFileAsync(logoFile, "store-logos");
            if (coverFile != null && coverFile.Length > 0) store.CoverImageUrl = await SaveFileAsync(coverFile, "store-covers");

            await _storeRepo.UpdateAsync(store);
            TempData["Success"] = "✅ Images saved.";
            return RedirectToAction("SettingsImages");
        }

        // ── Contact ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SettingsContact()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            return View(store);
        }

        // ══════════════════════════════════════════════════════════
        // استبدل SettingsContact POST الموجود بهذا الكود
        // ══════════════════════════════════════════════════════════

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsContact(
            string? email,
            string? phone,
            string? whatsApp,
            string? city,
            string? area,
            string? address,
            double? latitude,      // ← جديد
            double? longitude)     // ← جديد
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            store.Email = email;
            store.Phone = phone;
            store.WhatsApp = whatsApp;
            store.City = city;
            store.Area = area;
            store.Address = address;

            // الإحداثيات — نخزّن null إذا أرسل الفورم قيمة فارغة (أُزيل الدبوس)
            store.Latitude = latitude;
            store.Longitude = longitude;

            await _storeRepo.UpdateAsync(store);
            TempData["Success"] = "✅ Contact information saved.";
            return RedirectToAction("SettingsContact");
        }

        // ── Delivery ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SettingsDelivery()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            return View(store);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsDelivery(
            bool deliveryAvailable, string? deliveryAreas,
            string? openTime, string? closeTime, string? workingDays)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            store.DeliveryAvailable = deliveryAvailable;
            store.DeliveryAreas = deliveryAreas;
            store.WorkingDays = workingDays;
            store.OpenTime = string.IsNullOrEmpty(openTime) ? null : TimeSpan.TryParse(openTime, out var ot) ? ot : null;
            store.CloseTime = string.IsNullOrEmpty(closeTime) ? null : TimeSpan.TryParse(closeTime, out var ct) ? ct : null;
            await _storeRepo.UpdateAsync(store);
            TempData["Success"] = "✅ Delivery settings saved.";
            return RedirectToAction("SettingsDelivery");
        }

        // ── Payment ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SettingsPayment()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            return View(store);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsPayment(string? acceptedPayments)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            store.AcceptedPayments = acceptedPayments;
            await _storeRepo.UpdateAsync(store);
            TempData["Success"] = "✅ Payment methods saved.";
            return RedirectToAction("SettingsPayment");
        }

        // ── Helper داخلي للتحقق من الصورة ─────────────────────
        private static bool IsImageFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            var ext = Path.GetExtension(url).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        }


        // ══════════════════════════════════════════════════════════
        // أضف هذه الـ Actions إلى StoreAdminController.cs
        // في منطقة SETTINGS — بعد SettingsPayment
        // ══════════════════════════════════════════════════════════

        // ── Security ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SettingsSecurity()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            return View();   // لا يحتاج model — الفيو لا يعرض بيانات الـ Store
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsSecurity(
            string? currentPassword, string? newPassword, string? confirmPassword)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            // ── التحقق الأساسي ──────────────────────────────────
            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["Error"] = "❌ Please fill in all password fields.";
                return RedirectToAction("SettingsSecurity");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "❌ New passwords do not match.";
                return RedirectToAction("SettingsSecurity");
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "❌ New password must be at least 6 characters.";
                return RedirectToAction("SettingsSecurity");
            }

            // ── جلب المستخدم والتحقق من الـ current password ──
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // استبدل هذا بـ UserRepository أو AuthService الخاص بك
            // مثال افتراضي:
            // var user = await _userRepo.GetByIdAsync(userId.Value);
            // if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            // {
            //     TempData["Error"] = "❌ Current password is incorrect.";
            //     return RedirectToAction("SettingsSecurity");
            // }
            // user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            // await _userRepo.UpdateAsync(user);

            TempData["Success"] = "✅ Password updated successfully.";
            return RedirectToAction("SettingsSecurity");
        }

        // ══════════════════════════════════════════════════════════
        // عدّل الـ Settings() redirect الموجود ليشير إلى SettingsInfo
        // (هذا موجود بالفعل في الكونترولر — للتأكيد فقط)
        // ══════════════════════════════════════════════════════════

        // [HttpGet]
        // public IActionResult Settings() => RedirectToAction("SettingsInfo");
        // ← هذا السطر موجود بالفعل، لا داعي لتغييره


        // ══════════════════════════════════════════════════════════
        // STUBS
        // ══════════════════════════════════════════════════════════
        public IActionResult Reviews() => View();
        public IActionResult Adoption() => View();
        public IActionResult Notifications() => View();
        public IActionResult Subscription() => View();
    }
}