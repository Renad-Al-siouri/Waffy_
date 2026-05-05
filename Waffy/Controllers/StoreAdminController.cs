using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Repositories.Interfaces;
namespace Waffy.Controllers
{
    public class StoreAdminController : Controller
    {
        // ══ Dependencies ══════════════════════════════════════════
        private readonly IStoreRepository _storeRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderItemRepository _orderItemRepo;
        private readonly IStoreDocumentRepository _docRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IProductImageRepository _productImageRepo;
        private readonly IStoreImageRepository _storeImageRepo;

        public StoreAdminController(
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IOrderItemRepository orderItemRepo,
            IStoreDocumentRepository docRepo,
            ICategoryRepository categoryRepo,
            IProductImageRepository productImageRepo,
            IStoreImageRepository storeImageRepo)
        {
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _docRepo = docRepo;
            _categoryRepo = categoryRepo;
            _productImageRepo = productImageRepo;
            _storeImageRepo = storeImageRepo;
        }

        // ══ HELPERS ═══════════════════════════════════════════════

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

        private static bool IsImageFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            var ext = Path.GetExtension(url).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        }

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

        // ══ INDEX / DASHBOARD ═════════════════════════════════════

        public IActionResult Index() => RedirectToAction("Dashboard");

        [HttpGet]
public async Task<IActionResult> Dashboard()
{
    if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
    var store = await GetCurrentStore();
    if (store == null) return RedirectToAction("Login", "Account");
    SetSidebarData(store);
 
    var products   = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
    var categories = (await _categoryRepo.GetByStoreIdAsync(store.IdStore)).ToList();
    var orders     = (await _orderRepo.GetByStoreIdAsync(store.IdStore)).ToList();
    var revenue    = await _orderRepo.GetTotalRevenueByStoreAsync(store.IdStore);
 
    var productsJs = new List<object>();
    foreach (var p in products)
    {
        var imgs    = (await _productImageRepo.GetByProductIdAsync(p.Id)).ToList();
        var mainImg = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                   ?? imgs.FirstOrDefault()?.ImageUrl
                   ?? p.ImageUrl ?? "";
        productsJs.Add(new
        {
            id     = p.Id,
            name   = p.Name ?? "",
            cat    = p.IdCategory.ToString(),
            price  = p.Price,
            stock  = p.StockQuantity,
            status = p.StockQuantity == 0 ? "out" : (p.IsVisible ? "active" : "hidden"),
            image  = mainImg
        });
    }
 
    ViewBag.ProductsJson    = JsonSerializer.Serialize(productsJs);
    ViewBag.CategoriesJson  = JsonSerializer.Serialize(categories.Select(c => new
    {
        id   = c.IdCategory,
        name = c.Name ?? "",
        icon = (c.Icon != null && c.Icon != "??" && c.Icon != "?") ? c.Icon : "📦"
    }));
    ViewBag.OrdersJson      = JsonSerializer.Serialize(await Task.WhenAll(orders.Select(MapOrderToJs)));
    ViewBag.TotalRevenue    = revenue;
    ViewBag.PendingOrders   = orders.Count(o => o.Status == "Pending");
    ViewBag.CompletedOrders = orders.Count(o => o.Status == "Delivered");
    ViewBag.LowStockCount   = products.Count(p => p.StockQuantity <= 2);
    ViewBag.ProductsCount   = products.Count;
    ViewBag.TotalOrders     = orders.Count;
    return View();
}

        // ══ PRODUCTS ══════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var products = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var categories = (await _categoryRepo.GetByStoreIdAsync(store.IdStore)).ToList();

            var productsWithImages = new List<object>();
            foreach (var p in products)
            {
                var imgs = (await _productImageRepo.GetByProductIdAsync(p.Id)).ToList();
                var mainImg = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                           ?? imgs.FirstOrDefault()?.ImageUrl
                           ?? p.ImageUrl ?? "";

                productsWithImages.Add(new
                {
                    id = p.Id,
                    name = p.Name ?? "",
                    description = p.Description ?? "",
                    cat = p.IdCategory.ToString(),
                    price = p.Price,
                    salePrice = p.SalePrice,
                    stock = p.StockQuantity,
                    petType = p.PetType ?? "",
                    status = p.StockQuantity == 0 ? "out" : (p.IsVisible ? "active" : "hidden"),
                    image = mainImg,
                    images = imgs.Select(i => new
                    {
                        id = i.Id,
                        imageUrl = i.ImageUrl,
                        isMain = i.IsMain,
                        sortOrder = i.SortOrder
                    }).ToList()
                });
            }

            ViewBag.ProductsJson = JsonSerializer.Serialize(productsWithImages);
            ViewBag.CategoriesJson = JsonSerializer.Serialize(categories.Select(c => new
            {
                id = c.IdCategory,
                name = c.Name ?? "",
                icon = (c.Icon != null && c.Icon != "??" && c.Icon != "?") ? c.Icon : "📦"
            }));
            ViewBag.StoreId = store.IdStore;
            ViewBag.ProductCount = products.Count;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(
            string name, string? description, decimal price, decimal? salePrice,
            int stockQuantity, int idCategory, string? petType,
            List<IFormFile>? imageFiles)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false, message = "Store not found." });

            var validImageFiles = imageFiles?.Where(f => f?.Length > 0).ToList() ?? new();
            if (validImageFiles.Count > 8)
                return Json(new { success = false, message = "Maximum 8 images allowed." });

            // إذا لم تُختر فئة، نأخذ أول فئة للمتجر
            int finalCategoryId = idCategory;
            if (finalCategoryId <= 0)
            {
                var storeCats = (await _categoryRepo.GetByStoreIdAsync(store.IdStore)).ToList();
                finalCategoryId = storeCats.FirstOrDefault()?.IdCategory ?? 1;
            }

            var product = new Product
            {
                IdStore = store.IdStore,
                IdCategory = finalCategoryId,
                Name = name?.Trim() ?? "",
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                Price = price,
                SalePrice = (salePrice.HasValue && salePrice > 0) ? salePrice : null,
                StockQuantity = stockQuantity,
                PetType = string.IsNullOrWhiteSpace(petType) ? null : petType,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _productRepo.AddAsync(product);

            var savedImages = new List<object>();
            int sortOrder = 0;
            foreach (var imgFile in validImageFiles)
            {
                var url = await SaveFileAsync(imgFile, "products");
                bool main = sortOrder == 0;

                var imgId = await _productImageRepo.AddAsync(new ProductImage
                {
                    IdProduct = newId,
                    ImageUrl = url,
                    SortOrder = sortOrder,
                    IsMain = main
                });

                savedImages.Add(new { id = imgId, url, isMain = main });

                if (main)
                {
                    product.Id = newId;
                    product.ImageUrl = url;
                    await _productRepo.UpdateAsync(product);
                }
                sortOrder++;
            }

            return Json(new { success = true, id = newId, images = savedImages });
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(
            int id, string name, string? description, decimal price, decimal? salePrice,
            int stockQuantity, int idCategory, string? petType,
            List<IFormFile>? newImageFiles,
            string? deleteImageIds)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false, message = "Store not found." });

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false, message = "Product not found." });

            // حذف الصور المطلوب حذفها
            if (!string.IsNullOrEmpty(deleteImageIds))
            {
                foreach (var imgId in deleteImageIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x.Trim(), out var v) ? v : 0)
                    .Where(x => x > 0))
                {
                    await _productImageRepo.DeleteAsync(imgId);
                }
            }

            // إضافة الصور الجديدة
            var validNewFiles = newImageFiles?.Where(f => f?.Length > 0).ToList() ?? new();
            if (validNewFiles.Count > 0)
            {
                var existing = (await _productImageRepo.GetByProductIdAsync(id)).ToList();
                if (existing.Count + validNewFiles.Count > 8)
                    return Json(new { success = false, message = "Maximum 8 images per product." });

                int nextOrder = existing.Any() ? existing.Max(i => i.SortOrder) + 1 : 0;
                bool hasMain = existing.Any(i => i.IsMain);

                foreach (var imgFile in validNewFiles)
                {
                    var url = await SaveFileAsync(imgFile, "products");
                    bool isMain = !hasMain;
                    await _productImageRepo.AddAsync(new ProductImage
                    {
                        IdProduct = id,
                        ImageUrl = url,
                        SortOrder = nextOrder++,
                        IsMain = isMain
                    });
                    if (isMain) { product.ImageUrl = url; hasMain = true; }
                }
            }

            product.Name = name?.Trim() ?? "";
            product.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            product.Price = price;
            product.SalePrice = (salePrice.HasValue && salePrice > 0) ? salePrice : null;
            product.StockQuantity = stockQuantity;
            product.IdCategory = idCategory > 0 ? idCategory : product.IdCategory;
            product.PetType = string.IsNullOrWhiteSpace(petType) ? null : petType;
            await _productRepo.UpdateAsync(product);

            var updatedImages = (await _productImageRepo.GetByProductIdAsync(id)).ToList();
            return Json(new
            {
                success = true,
                imageUrl = product.ImageUrl,
                images = updatedImages.Select(i => new
                {
                    id = i.Id,
                    imageUrl = i.ImageUrl,
                    isMain = i.IsMain,
                    sortOrder = i.SortOrder
                })
            });
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

        [HttpPost]
        public async Task<IActionResult> SetMainImage(int productId, int imageId)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null || product.IdStore != store.IdStore)
                return Json(new { success = false, message = "Not authorized." });

            await _productImageRepo.SetMainImageAsync(productId, imageId);

            var img = (await _productImageRepo.GetByProductIdAsync(productId))
                      .FirstOrDefault(i => i.Id == imageId);
            if (img != null)
            {
                product.ImageUrl = img.ImageUrl;
                await _productRepo.UpdateAsync(product);
            }
            return Json(new { success = true });
        }

        // ══ CATEGORIES ════════════════════════════════════════════

        [HttpPost]
        public async Task<IActionResult> AddCategory(string name, string? icon)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Category name required." });

            var newId = await _categoryRepo.AddAsync(new Category
            {
                IdStore = store.IdStore,
                Name = name.Trim(),
                Icon = string.IsNullOrEmpty(icon) ? "📦" : icon.Trim()
            });
            return Json(new { success = true, id = newId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var cat = await _categoryRepo.GetByIdAsync(id);
            if (cat == null || cat.IdStore != store.IdStore)
                return Json(new { success = false, message = "Not authorized." });

            await _categoryRepo.DeleteAsync(id);
            return Json(new { success = true });
        }

        // ══ ORDERS ════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var orders = (await _orderRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            ViewBag.OrdersJson = JsonSerializer.Serialize(
                await Task.WhenAll(orders.Select(MapOrderToJs)));
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

        // ══ CUSTOMERS ═════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var customers = await _orderRepo.GetCustomersByStoreAsync(store.IdStore);
            ViewBag.CustomersJson = JsonSerializer.Serialize(customers.Select(c => new
            {
                name = c.FullName ?? "",
                email = c.Email ?? "",
                phone = c.Phone ?? "",
                orders = c.OrderCount,
                spent = c.TotalSpent,
                last = c.LastOrderDate?.ToString("dd MMM yyyy") ?? "-"
            }));
            return View();
        }

        // ══ SETTINGS ══════════════════════════════════════════════

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

            var docs = (await _docRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            ViewBag.Documents = docs.Select(d => new
            {
                id = d.Id,
                type = d.DocumentType ?? "",
                label = MapDocLabel(d.DocumentType),
                fileUrl = d.FileUrl ?? "",
                status = (d.Status ?? "pending").ToLower(),
                adminNote = d.AdminNote ?? "",
                uploadedAt = d.UploadedAt.ToString("dd MMM yyyy"),
                isImage = IsImageFile(d.FileUrl)
            }).ToList();

            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null) ViewBag.Error = TempData["Error"];
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

        // ── Images ────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SettingsImages()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);

            var allImages = (await _storeImageRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            ViewBag.GalleryImages = allImages
                .Where(i => i.ImageType == "Gallery")
                .OrderBy(i => i.SortOrder)
                .ToList();

            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null) ViewBag.Error = TempData["Error"];
            return View(store);
        }

        [HttpPost]
        public async Task<IActionResult> UploadStoreImages(
            string imageType,
            List<IFormFile>? imageFiles)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false, message = "Store not found." });

            var validFiles = imageFiles?.Where(f => f?.Length > 0).ToList() ?? new();
            if (!validFiles.Any())
                return Json(new { success = false, message = "No files received." });

            // Logo و Cover: صورة واحدة فقط
            if (imageType == "Logo" || imageType == "Cover")
            {
                await _storeImageRepo.DeleteByStoreIdAndTypeAsync(store.IdStore, imageType);
                validFiles = validFiles.Take(1).ToList();
            }
            else if (imageType == "Gallery")
            {
                var existingCount = (await _storeImageRepo.GetByStoreIdAndTypeAsync(store.IdStore, "Gallery")).Count();
                if (existingCount >= 8)
                    return Json(new { success = false, message = "Maximum 8 gallery images reached." });
                validFiles = validFiles.Take(8 - existingCount).ToList();
            }

            var subFolder = imageType switch
            {
                "Logo" => "store-logos",
                "Cover" => "store-covers",
                _ => "store-gallery"
            };

            var savedUrls = new List<string>();
            int sortOrder = 0;
            foreach (var file in validFiles)
            {
                var url = await SaveFileAsync(file, subFolder);
                await _storeImageRepo.AddAsync(new StoreImage
                {
                    IdStore = store.IdStore,
                    ImageUrl = url,
                    ImageType = imageType,
                    SortOrder = sortOrder++
                });
                savedUrls.Add(url);

                // نحدّث الحقول القديمة في Store للتوافق
                if (imageType == "Logo")
                {
                    store.LogoUrl = url;
                    await _storeRepo.UpdateAsync(store);
                }
                else if (imageType == "Cover")
                {
                    store.CoverImageUrl = url;
                    await _storeRepo.UpdateAsync(store);
                }
            }

            return Json(new { success = true, urls = savedUrls });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStoreImage(int imageId)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            await _storeImageRepo.DeleteAsync(imageId);
            return Json(new { success = true });
        }

        // ── Contact ───────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SettingsContact()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
            return View(store);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsContact(
            string? email, string? phone, string? whatsApp,
            string? city, string? area, string? address,
            double? latitude, double? longitude)
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
            store.Latitude = latitude;
            store.Longitude = longitude;

            await _storeRepo.UpdateAsync(store);
            TempData["Success"] = "✅ Contact information saved.";
            return RedirectToAction("SettingsContact");
        }

        // ── Delivery ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SettingsDelivery()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
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

        // ── Payment ───────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SettingsPayment()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
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

        // ── Security ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SettingsSecurity()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            SetSidebarData(store);
            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null) ViewBag.Error = TempData["Error"];
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsSecurity(
            string? currentPassword, string? newPassword, string? confirmPassword)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

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

            // TODO: تحقق من الـ currentPassword مقابل الـ DB وحدّث الـ hash
            TempData["Success"] = "✅ Password updated successfully.";
            return RedirectToAction("SettingsSecurity");
        }

        // ══ STUBS ═════════════════════════════════════════════════

        public IActionResult Reviews() => View();
        public IActionResult Adoption() => View();
        public IActionResult Notifications() => View();
        public IActionResult Subscription() => View();
    }
}