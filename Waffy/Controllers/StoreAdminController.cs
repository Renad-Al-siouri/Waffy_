using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models;

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
        private readonly IComplaintRepository _complaintRepo;
        private readonly IAdoptionRepository _adoptionRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly IWalletRepository _walletRepo;
        private readonly IStoreWalletRepository _storeWalletRepo;
        public StoreAdminController(
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IOrderItemRepository orderItemRepo,
            IStoreDocumentRepository docRepo,
            ICategoryRepository categoryRepo,
            IProductImageRepository productImageRepo,
            IStoreImageRepository storeImageRepo,
            IComplaintRepository complaintRepo,
            IAdoptionRepository adoptionRepo,
             IWalletRepository walletRepo,
             IStoreWalletRepository storeWalletRepo,
            INotificationRepository notifRepo) 
        {
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _docRepo = docRepo;
            _categoryRepo = categoryRepo;
            _productImageRepo = productImageRepo;
            _storeImageRepo = storeImageRepo;
            _complaintRepo = complaintRepo;
            _adoptionRepo = adoptionRepo;
            _walletRepo = walletRepo;
            _notifRepo = notifRepo;
            _storeWalletRepo = storeWalletRepo;
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

        private async Task SetSidebarDataAsync(Store store)
        {
            string ownerName = HttpContext.Session.GetString("FullName") ?? "Store Owner";
            ViewBag.StoreName = store.Name ?? "";
            ViewBag.OwnerName = ownerName;
            ViewBag.OwnerInitial = ownerName.Length > 0 ? ownerName[0].ToString().ToUpper() : "S";

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.NotifCount = userId.HasValue
                ? await _notifRepo.GetUnreadCountAsync(userId.Value) : 0;

            var orders = await _orderRepo.GetByStoreIdAsync(store.IdStore);
            var complaints = await _complaintRepo.GetByStoreIdAsync(store.IdStore);
            var products = await _productRepo.GetByStoreIdAsync(store.IdStore);

           
        }

        private async Task CheckAndNotifyLowStockAsync(Store store)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return;

            var products = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            foreach (var p in products)
            {
                if (p.StockQuantity == 0)
                {
                    await _notifRepo.AddAsync(new Notification
                    {
                        IdUser = userId.Value,
                        Type = "OutOfStock",
                        Title = "⚠️ Product Out of Stock",
                        Message = $"\"{p.Name}\" is now out of stock.",
                        IsRead = false,
                        RelatedEntityType = "Product",
                        RelatedEntityId = p.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (p.StockQuantity <= 3)
                {
                    await _notifRepo.AddAsync(new Notification
                    {
                        IdUser = userId.Value,
                        Type = "LowStock",
                        Title = "📦 Low Stock Alert",
                        Message = $"\"{p.Name}\" has only {p.StockQuantity} unit(s) left.",
                        IsRead = false,
                        RelatedEntityType = "Product",
                        RelatedEntityId = p.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
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

        /// <summary>
        /// Maps an Order to the JS-friendly object used by the frontend.
        /// Includes per-item product images and delivery-person info.
        /// </summary>
        private async Task<object> MapOrderToJs(Order o)
        {
            // Uses IOrderRepository.GetItemsByOrderIdAsync — avoids IOrderItemRepository method name mismatch
            var rawItems = (await _orderRepo.GetItemsByOrderIdAsync(o.IdOrder)).ToList();

            var itemDetails = new List<object>();
            foreach (var i in rawItems)
            {
                string mainImg = "";
                try
                {
                    var imgs = (await _productImageRepo.GetByProductIdAsync(i.IdProduct)).ToList();
                    mainImg = imgs.FirstOrDefault(x => x.IsMain)?.ImageUrl
                            ?? imgs.FirstOrDefault()?.ImageUrl ?? "";
                }
                catch { /* image fetch is best-effort */ }

                itemDetails.Add(new
                {
                    name = i.ProductNameSnapshot ?? "Product",
                    qty = i.Quantity,
                    price = i.SalePrice ?? i.UnitPrice,
                    image = mainImg
                });
            }

            return new
            {
                idOrder = o.IdOrder,
                id = o.StoreOrderNumber > 0 ? o.StoreOrderNumber.ToString() : o.IdOrder.ToString(),
                cust = o.RecipientName ?? "—",
                phone = o.RecipientPhone ?? "",
                address = o.DeliveryAddress ?? "",
                date = o.CreatedAt.ToString("dd MMM yyyy"),
                prods = rawItems.Count,
                total = rawItems.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice)),
                deliveryFee = o.DeliveryFee,
                grandTotal = rawItems.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice)) + o.DeliveryFee,
                status = (o.Status ?? "pending").ToLower(),
                deliveryPersonName = o.DeliveryPersonName ?? "",
                deliveryPersonPhone = o.DeliveryPersonPhone ?? "",
                paymentMethod = o.PaymentMethod ?? "",
                cliqCode = o.CliQVerificationCode ?? "",  // ← جديد
                cliqVerified = o.CliQVerified,
                items = itemDetails
            };
        }

        // ══ INDEX ═════════════════════════════════════════════════

        public IActionResult Index() => RedirectToAction("Dashboard");

        // ══ DASHBOARD ═════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Dashboard";
            ViewData["ActiveNav"] = "Dashboard";
            ViewData["BreadSection"] = "Store";
            ViewData["BreadPage"] = "Dashboard";

            var products = (await _productRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var categories = (await _categoryRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var orders = (await _orderRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            var (itemsRevenue, deliveryRevenue) = await _orderRepo.GetTotalRevenueByStoreAsync(store.IdStore);
            ViewBag.ItemsRevenue = itemsRevenue;
            ViewBag.DeliveryRevenue = deliveryRevenue;
            ViewBag.TotalRevenue = itemsRevenue + deliveryRevenue;
            // Build products JSON (with main image)
            var productsJs = new List<object>();
            foreach (var p in products)
            {
                var imgs = (await _productImageRepo.GetByProductIdAsync(p.Id)).ToList();
                var mainImg = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                           ?? imgs.FirstOrDefault()?.ImageUrl
                           ?? p.ImageUrl ?? "";
                productsJs.Add(new
                {
                    id = p.Id,
                    name = p.Name ?? "",
                    cat = p.IdCategory.ToString(),
                    price = p.Price,
                    stock = p.StockQuantity,
                    status = p.StockQuantity == 0 ? "out" : (p.IsVisible ? "active" : "hidden"),
                    image = mainImg
                });
            }

            ViewBag.ProductsJson = JsonSerializer.Serialize(productsJs);
            ViewBag.CategoriesJson = JsonSerializer.Serialize(categories.Select(c => new
            {
                id = c.IdCategory,
                name = c.Name ?? "",
                icon = (c.Icon != null && c.Icon != "??" && c.Icon != "?") ? c.Icon : "📦"
            }));
            ViewBag.OrdersJson = JsonSerializer.Serialize(
                                          await Task.WhenAll(orders.Select(MapOrderToJs)));
            ViewBag.TotalRevenue = itemsRevenue + deliveryRevenue;
            ViewBag.PendingOrders = orders.Count(o => o.Status == "Pending");
            ViewBag.CompletedOrders = orders.Count(o => o.Status == "Delivered");
            ViewBag.LowStockCount = products.Count(p => p.StockQuantity <= 2);
            ViewBag.ProductsCount = products.Count;
            ViewBag.TotalOrders = orders.Count;

            return View();
        }

        // ══ PRODUCTS ══════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Products";
            ViewData["ActiveNav"] = "Products";
            ViewData["BreadSection"] = "Store";
            ViewData["BreadPage"] = "Products";

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

            var validFiles = imageFiles?.Where(f => f?.Length > 0).ToList() ?? new();
            if (validFiles.Count > 8)
                return Json(new { success = false, message = "Maximum 8 images allowed." });

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

            foreach (var imgFile in validFiles)
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
            if (!IsStoreOwner()) return RedirectToAction("Products");

            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Products");

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || product.IdStore != store.IdStore)
                return RedirectToAction("Products");

            try
            {
                // استدعاء عملية الحذف الذكية من الـ Repository
                await _productRepo.DeleteAsync(id);

                // السر هنا: إعادة التوجيه لصفحة المنتجات لتحديث الجدول برمجياً وتلقائياً فوراً
                return RedirectToAction("Products");
            }
            catch
            {
                // في حال حدوث أي خطأ، يتم إنعاش الصفحة أيضاً لمنع الكراش
                return RedirectToAction("Products");
            }
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
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Orders";
            ViewData["ActiveNav"] = "Orders";
            ViewData["BreadSection"] = "Store";
            ViewData["BreadPage"] = "Orders";

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

            // ── إشعار لليوزر ──────────────────────────────────────
            var (icon, title, message) = dbStatus switch
            {
                "Confirmed" => ("✅", "Order Confirmed", $"Your order #{order.OrderCode} has been confirmed."),
                "Preparing" => ("👨‍🍳", "Order Being Prepared", $"Your order #{order.OrderCode} is being prepared."),
                "Shipped" => ("🚚", "Order Shipped", $"Your order #{order.OrderCode} is on its way!"),
                "Delivered" => ("📦", "Order Delivered", $"Your order #{order.OrderCode} has been delivered!"),
                "Cancelled" => ("❌", "Order Cancelled", $"Your order #{order.OrderCode} has been cancelled."),
                _ => ("🔔", "Order Updated", $"Your order #{order.OrderCode} status: {dbStatus}.")
            };

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = order.IdUser,
                Type = "Order" + dbStatus,
                Title = icon + " " + title,
                Message = message,
                IsRead = false,
                RelatedEntityType = "Order",
                RelatedEntityId = idOrder,
                CreatedAt = DateTime.UtcNow
            });

            // ══ DELIVERED ══════════════════════════════════════════
            if (dbStatus == "Delivered")
            {
                // CliQ: حرّر الضمان المحجوز وأرجعه لمحفظة المتجر
                if ((order.PaymentMethod ?? "").ToLower() == "cliq" && order.CliQVerified)
                {
                    var items2 = (await _orderRepo.GetItemsByOrderIdAsync(idOrder)).ToList();
                    var total2 = items2.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice)) + order.DeliveryFee;
                    var sw = await _storeWalletRepo.GetOrCreateAsync(store.IdStore);

                    await _storeWalletRepo.AddBalanceAsync(sw.IdStoreWallet, total2);
                    await _storeWalletRepo.AddTransactionAsync(new StoreWalletTransaction
                    {
                        IdStoreWallet = sw.IdStoreWallet,
                        Type = "CliQReleased",
                        Amount = total2,
                        Notes = $"Guarantee released — order #{order.OrderCode} delivered successfully",
                        RelatedOrderId = idOrder,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                // Wallet: المال موجود بالفعل في محفظة المتجر منذ الطلب → لا شيء إضافي

                await CheckAndNotifyLowStockAsync(store);
            }

            // ══ CANCELLED ══════════════════════════════════════════
            if (dbStatus == "Cancelled")
            {
                var items = (await _orderRepo.GetItemsByOrderIdAsync(idOrder)).ToList();
                var total = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice)) + order.DeliveryFee;

                // ── إعادة المخزون ──────────────────────────────────
                foreach (var item in items)
                    await _productRepo.RestoreStockAsync(item.IdProduct, item.Quantity);

                var payMethod = (order.PaymentMethod ?? "").ToLower();
                bool shouldRefund = false;

                // ── Wallet: المال في محفظة المتجر → خصم منها وأرجع لليوزر
                if (payMethod == "wallet")
                {
                    var storeWallet = await _storeWalletRepo.GetOrCreateAsync(store.IdStore);

                    if (storeWallet.Balance >= total)
                    {
                        await _storeWalletRepo.DeductBalanceAsync(storeWallet.IdStoreWallet, total);
                        await _storeWalletRepo.AddTransactionAsync(new StoreWalletTransaction
                        {
                            IdStoreWallet = storeWallet.IdStoreWallet,
                            Type = "Refund",
                            Amount = -total,
                            Notes = $"Refund for cancelled wallet order #{order.OrderCode}",
                            RelatedOrderId = idOrder,
                            CreatedAt = DateTime.UtcNow
                        });
                        shouldRefund = true;
                    }
                    else
                    {
                        // حالة نادرة جداً — المتجر سحب المال قبل الإلغاء
                        await _notifRepo.AddAsync(new Notification
                        {
                            IdUser = store.OwnerUserId,
                            Type = "WalletInsufficient",
                            Title = "⚠️ Insufficient Store Wallet",
                            Message = $"Cannot auto-refund order #{order.OrderCode} ({total:F3} JD). " +
                                               $"Store wallet balance: {storeWallet.Balance:F3} JD. Please refund customer manually.",
                            IsRead = false,
                            RelatedEntityType = "Order",
                            RelatedEntityId = idOrder,
                            CreatedAt = DateTime.UtcNow
                        });
                        await _notifRepo.AddAsync(new Notification
                        {
                            IdUser = order.IdUser,
                            Type = "RefundPending",
                            Title = "⏳ Refund Pending",
                            Message = $"Your order #{order.OrderCode} was cancelled. The store will contact you to process your refund manually.",
                            IsRead = false,
                            RelatedEntityType = "Order",
                            RelatedEntityId = idOrder,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                // ── CliQ مؤكد: الضمان محجوز في المتجر → خصم وأرجع لليوزر
                else if (payMethod == "cliq" && order.CliQVerified)
                {
                    var storeWallet = await _storeWalletRepo.GetOrCreateAsync(store.IdStore);

                    if (storeWallet.Balance >= total)
                    {
                        await _storeWalletRepo.DeductBalanceAsync(storeWallet.IdStoreWallet, total);
                        await _storeWalletRepo.AddTransactionAsync(new StoreWalletTransaction
                        {
                            IdStoreWallet = storeWallet.IdStoreWallet,
                            Type = "Refund",
                            Amount = -total,
                            Notes = $"CliQ guarantee refund for cancelled order #{order.OrderCode}",
                            RelatedOrderId = idOrder,
                            CreatedAt = DateTime.UtcNow
                        });
                        shouldRefund = true;
                    }
                    else
                    {
                        await _notifRepo.AddAsync(new Notification
                        {
                            IdUser = store.OwnerUserId,
                            Type = "WalletInsufficient",
                            Title = "⚠️ Insufficient Store Wallet",
                            Message = $"Cannot refund order #{order.OrderCode} ({total:F3} JD). " +
                                               $"Store wallet: {storeWallet.Balance:F3} JD. Please top up and refund customer manually.",
                            IsRead = false,
                            RelatedEntityType = "Order",
                            RelatedEntityId = idOrder,
                            CreatedAt = DateTime.UtcNow
                        });
                        await _notifRepo.AddAsync(new Notification
                        {
                            IdUser = order.IdUser,
                            Type = "RefundPending",
                            Title = "⏳ Refund Pending",
                            Message = $"Your order #{order.OrderCode} was cancelled. The store will contact you to process your CliQ refund manually.",
                            IsRead = false,
                            RelatedEntityType = "Order",
                            RelatedEntityId = idOrder,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                // ── Cash: لا دفع مسبق → لا استرداد ────────────────
                // else: cash → do nothing

                // ── أضف المبلغ لمحفظة اليوزر إذا تم الاسترداد ────
                if (shouldRefund && total > 0)
                {
                    var userWallet = await _walletRepo.GetByUserIdAsync(order.IdUser);
                    if (userWallet != null)
                    {
                        await _walletRepo.AddBalanceAsync(userWallet.IdWallet, total);
                        await _walletRepo.AddTransactionAsync(new WalletTransaction
                        {
                            IdWallet = userWallet.IdWallet,
                            TransactionCode = $"REFUND-{order.OrderCode}",
                            Type = "Refund",
                            Amount = total,
                            PaymentMethod = "Wallet",
                            ReferenceId = idOrder,
                            Status = "Completed",
                            CreatedAt = DateTime.UtcNow
                        });
                        await _notifRepo.AddAsync(new Notification
                        {
                            IdUser = order.IdUser,
                            Type = "OrderRefund",
                            Title = "💚 Refund Processed",
                            Message = $"Your order #{order.OrderCode} was cancelled. {total:F3} JD has been refunded to your Waffy wallet.",
                            IsRead = false,
                            RelatedEntityType = "Order",
                            RelatedEntityId = idOrder,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Sets the delivery person details and marks the order as Shipped.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryPerson(
            int idOrder, string deliveryPersonName, string deliveryPersonPhone)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var order = await _orderRepo.GetByIdAsync(idOrder);
            if (order == null || order.IdStore != store.IdStore)
                return Json(new { success = false, message = "Order not found." });

            if (string.IsNullOrWhiteSpace(deliveryPersonName) ||
                string.IsNullOrWhiteSpace(deliveryPersonPhone))
                return Json(new { success = false, message = "Delivery person details are required." });

            await _orderRepo.UpdateDeliveryPersonAsync(
                idOrder,
                deliveryPersonName.Trim(),
                deliveryPersonPhone.Trim());

            return Json(new { success = true });
        }

        // ══ CUSTOMERS ═════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Customers";
            ViewData["ActiveNav"] = "Customers";
            ViewData["BreadSection"] = "Store";
            ViewData["BreadPage"] = "Customers";

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
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Store Settings — Info";
            ViewData["ActiveNav"] = "Settings";
            ViewData["ActiveSettings"] = "Info";
            ViewData["BreadSection"] = "Settings";
            ViewData["BreadPage"] = "Store Info";

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
        public async Task<IActionResult> SettingsInfo(
     string name, string? description, string? storeType,
     string? openTime, string? closeTime)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            store.Name = name;
            store.Description = description;
            store.StoreType = storeType;
            store.OpenTime = string.IsNullOrEmpty(openTime) ? null
                                : TimeSpan.TryParse(openTime, out var ot) ? ot : null;
            store.CloseTime = string.IsNullOrEmpty(closeTime) ? null
                                : TimeSpan.TryParse(closeTime, out var ct) ? ct : null;

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
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Store Settings — Images";
            ViewData["ActiveNav"] = "Settings";
            ViewData["ActiveSettings"] = "Images";
            ViewData["BreadSection"] = "Settings";
            ViewData["BreadPage"] = "Images & Gallery";

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
        public async Task<IActionResult> UploadStoreImages(string imageType, List<IFormFile>? imageFiles)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false, message = "Store not found." });

            var validFiles = imageFiles?.Where(f => f?.Length > 0).ToList() ?? new();
            if (!validFiles.Any()) return Json(new { success = false, message = "No files received." });

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
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Store Settings — Contact";
            ViewData["ActiveNav"] = "Settings";
            ViewData["ActiveSettings"] = "Contact";
            ViewData["BreadSection"] = "Settings";
            ViewData["BreadPage"] = "Contact & Location";

            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
            return View(store);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsContact(
     string? email, string? phone, string? whatsApp,
     string? city, string? area, string? address,
     double? latitude, double? longitude,
     string? cliqNumber)
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
            store.CliQNumber = string.IsNullOrWhiteSpace(cliqNumber) ? null : cliqNumber.Trim();

            await _storeRepo.UpdateAsync(store); // ← مرة واحدة فقط

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
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Store Settings — Delivery";
            ViewData["ActiveNav"] = "Settings";
            ViewData["ActiveSettings"] = "Delivery";
            ViewData["BreadSection"] = "Settings";
            ViewData["BreadPage"] = "Delivery Settings";

            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
            return View(store);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingsDelivery(
             bool deliveryAvailable, string? deliveryAreas,
    string? openTime, string? closeTime, string? workingDays,
    decimal? deliveryFee)
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");

            store.DeliveryAvailable = deliveryAvailable;
            store.DeliveryFee = deliveryFee ?? 0;
            store.DeliveryAreas = deliveryAreas;
            store.WorkingDays = workingDays;
            store.OpenTime = string.IsNullOrEmpty(openTime) ? null : TimeSpan.TryParse(openTime, out var ot) ? ot : null;
            store.CloseTime = string.IsNullOrEmpty(closeTime) ? null : TimeSpan.TryParse(closeTime, out var ct) ? ct : null;

            await _storeRepo.UpdateAsync(store);
            TempData["Success"] = "✅ Delivery settings saved.";
            return RedirectToAction("SettingsDelivery");
        }

        // ── Security ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SettingsSecurity()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Store Settings — Security";
            ViewData["ActiveNav"] = "Settings";
            ViewData["ActiveSettings"] = "Security";
            ViewData["BreadSection"] = "Settings";
            ViewData["BreadPage"] = "Account Security";

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

            // TODO: verify currentPassword against DB hash, update with new hash
            TempData["Success"] = "✅ Password updated successfully.";
            return RedirectToAction("SettingsSecurity");
        }

        // ══ STUB PAGES ════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Complaints";
            ViewData["ActiveNav"] = "Reviews";
            ViewData["BreadSection"] = "Store";
            ViewData["BreadPage"] = "Complaints";

            var complaints = (await _complaintRepo.GetByStoreIdAsync(store.IdStore)).ToList();
            ViewBag.ComplaintsJson = JsonSerializer.Serialize(complaints.Select(c => new
            {
                id = c.IdComplaint,
                complainant = c.ComplainantName ?? "Anonymous",
                title = c.Title ?? "",
                description = c.Description ?? "",
                status = (c.Status ?? "open").ToLower(),
                priority = c.Priority ?? "Medium",
                date = c.CreatedAt?.ToString("dd MMM yyyy · hh:mm tt") ?? "",
                source = c.IdUser.HasValue ? "User" : "Admin"
            }));

            ViewBag.TotalCount = complaints.Count;
            ViewBag.OpenCount = complaints.Count(c => c.Status == "Open");
            ViewBag.ResolvedCount = complaints.Count(c => c.Status == "Resolved");

            return View();
        }

        // ── Update complaint status (AJAX) ────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateComplaintStatus(int id, string status)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            await _complaintRepo.UpdateStatusAsync(id, status);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Adoption()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Adoption";
            ViewData["ActiveNav"] = "Adoption";
            ViewData["BreadSection"] = "Store";
            ViewData["BreadPage"] = "Adoption";

            var posts = (await _adoptionRepo.GetByStoreIdAsync(store.IdStore)).ToList();

            // جلب صورة لكل post
            var postData = new List<object>();
            foreach (var p in posts)
            {
                var imgs = (await _adoptionRepo.GetAllImagesAsync(p.IdPost)).ToList();
                postData.Add(new
                {
                    id = p.IdPost,
                    name = p.PetName ?? "",
                    animalType = p.AnimalType ?? "",
                    breed = p.Breed ?? "",
                    age = p.Age,
                    gender = p.Gender ?? "",
                    city = p.City ?? "",
                    healthStatus = p.HealthStatus ?? "",
                    isVaccinated = p.IsVaccinated,
                    isNeutered = p.IsNeutered,
                    goodWithChildren = p.GoodWithChildren,
                    goodWithPets = p.GoodWithPets,
                    energyLevel = p.EnergyLevel ?? "",
                    adoptionStatus = (p.AdoptionStatus ?? "Available").ToLower(),
                    status = (p.Status ?? "Approved").ToLower(),
                    description = p.Description ?? "",
                    contactPhone = p.ContactPhone ?? "",
                    postCode = p.PostCode ?? "",
                    image = imgs.FirstOrDefault() ?? "",
                    images = imgs,                          // ← كل الصور
                    createdAt = p.CreatedAt.ToString("dd MMM yyyy")
                });
            }

            ViewBag.PetsJson = System.Text.Json.JsonSerializer.Serialize(postData);
            ViewBag.TotalCount = posts.Count;
            ViewBag.AvailableCount = posts.Count(p => p.AdoptionStatus == "Available");
            ViewBag.AdoptedCount = posts.Count(p => p.AdoptionStatus == "Adopted");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAdoptionPet(
            string petName, string animalType, string? breed, string? ageText,
            string? gender, string? color, string? healthStatus,
            bool isVaccinated, bool isNeutered, bool goodWithChildren, bool goodWithPets,
            string? energyLevel, string? description, string? contactPhone,
            string? traits, List<IFormFile>? petImages)
        {
            if (!IsStoreOwner()) return Json(new { success = false, message = "Not authorized." });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            if (string.IsNullOrWhiteSpace(petName))
                return Json(new { success = false, message = "Pet name is required." });

            var post = new AdoptionPost
            {
                PostedByType = "Store",
                PostedById = store.IdStore,
                IdPet = null,                      // stores have no Pet record
                StoreName = store.Name,
                PetName = petName.Trim(),
                AnimalType = animalType?.Trim(),
                Breed = breed?.Trim(),
                Age = int.TryParse(ageText, out var a) ? a : null,
                Gender = gender,
                Color = color?.Trim(),
                HealthStatus = healthStatus?.Trim(),
                IsVaccinated = isVaccinated,
                IsNeutered = isNeutered,
                GoodWithChildren = goodWithChildren,
                GoodWithPets = goodWithPets,
                EnergyLevel = energyLevel,
                Description = description?.Trim(),
                ContactName = store.Name,
                ContactPhone = contactPhone?.Trim() ?? store.Phone ?? "",
                City = store.City ?? "",
                Area = store.Area ?? "",
                ReasonForAdoption = "Store adoption listing",
                Status = "Approved",                // ← no approval needed for verified stores
                AdoptionStatus = "Available",
                PostCode = $"WAFFY-STR-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _adoptionRepo.AddAsync(post);

            // حفظ الصور
            var validImages = petImages?.Where(f => f?.Length > 0).Take(5).ToList() ?? new();
            foreach (var img in validImages)
            {
                var url = await SaveFileAsync(img, "adoption");
                if (!string.IsNullOrEmpty(url))
                    await _adoptionRepo.AddImageAsync(newId, url);
            }

            return Json(new { success = true, id = newId, postCode = post.PostCode });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAdoptionStatus(int postId, string status)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var post = await _adoptionRepo.GetByIdAsync(postId);
            if (post == null || post.PostedById != store.IdStore)
                return Json(new { success = false, message = "Not authorized." });

            await _adoptionRepo.ChangeAdoptionStatusAsync(postId, status);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdoptionPost(int postId)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var post = await _adoptionRepo.GetByIdAsync(postId);
            if (post == null || post.PostedById != store.IdStore)
                return Json(new { success = false, message = "Not authorized." });

            await _adoptionRepo.DeleteAsync(postId);
            return Json(new { success = true });
        }

       
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var notifs = (await _notifRepo.GetByUserIdAsync(userId)).ToList();

            ViewBag.NotificationsJson = System.Text.Json.JsonSerializer.Serialize(
                notifs.Select(n => new
                {
                    id = n.Id,
                    type = n.Type ?? "",
                    title = n.Title ?? "",
                    message = n.Message ?? "",
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt,
                    relatedType = n.RelatedEntityType ?? "",
                    relatedId = n.RelatedEntityId
                }));

            ViewBag.UnreadCount = notifs.Count(n => !n.IsRead);
            ViewBag.TotalCount = notifs.Count;
            return View();
        }
        // أضف هذين الـ endpoints
        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            await _notifRepo.MarkAsReadAsync(id);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue) await _notifRepo.MarkAllAsReadAsync(userId.Value);
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> Subscription()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);
            ViewData["Title"] = "Subscription";
            ViewData["ActiveNav"] = "Subscription";
            ViewData["BreadSection"] = "Management";
            ViewData["BreadPage"] = "Subscription & Payments";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditAdoptionPost(
    int postId, string petName, string? breed, string? ageText,
    string? gender, string? color, string? healthStatus,
    bool isVaccinated, bool isNeutered, bool goodWithChildren, bool goodWithPets,
    string? energyLevel, string? description, string? contactPhone, string? traits)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var post = await _adoptionRepo.GetByIdAsync(postId);
            if (post == null || post.PostedById != store.IdStore)
                return Json(new { success = false, message = "Not authorized." });

            post.PetName = petName.Trim();
            post.Breed = breed?.Trim();
            post.Age = int.TryParse(ageText, out var a) ? a : post.Age;
            post.Gender = gender;
            post.Color = color?.Trim();
            post.HealthStatus = healthStatus?.Trim();
            post.IsVaccinated = isVaccinated;
            post.IsNeutered = isNeutered;
            post.GoodWithChildren = goodWithChildren;
            post.GoodWithPets = goodWithPets;
            post.EnergyLevel = energyLevel;
            post.Description = description?.Trim();
            post.ContactPhone = contactPhone?.Trim();

            await _adoptionRepo.UpdateAsync(post);
            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmCliQPayment(int idOrder)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            var order = await _orderRepo.GetByIdAsync(idOrder);
            if (order == null || order.IdStore != store.IdStore)
                return Json(new { success = false, message = "Order not found." });

            var items = (await _orderRepo.GetItemsByOrderIdAsync(idOrder)).ToList();
            var total = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice)) + order.DeliveryFee;

            // ── تحقق من رصيد محفظة المتجر ──────────────────────────
            var storeWallet = await _storeWalletRepo.GetOrCreateAsync(store.IdStore);
            if (storeWallet.Balance < total)
            {
                return Json(new
                {
                    success = false,
                    message = $"Your store wallet balance ({storeWallet.Balance:F3} JD) is insufficient to guarantee this refund ({total:F3} JD). " +
                              $"Please top up your wallet by at least {(total - storeWallet.Balance):F3} JD before confirming."
                });
            }

            // ── خصم من محفظة المتجر كـ "ضمان" ──────────────────────
            await _storeWalletRepo.DeductBalanceAsync(storeWallet.IdStoreWallet, total);
            await _storeWalletRepo.AddTransactionAsync(new StoreWalletTransaction
            {
                IdStoreWallet = storeWallet.IdStoreWallet,
                Type = "CliQGuarantee",
                Amount = -total,
                Notes = $"Guarantee held for CliQ order #{order.OrderCode}",
                RelatedOrderId = idOrder,
                CreatedAt = DateTime.UtcNow
            });

            // ── تأكيد الدفع وتغيير الستاتوس ────────────────────────
            await _orderRepo.UpdateCliQVerifiedAsync(idOrder, true);
            await _orderRepo.UpdateStatusAsync(idOrder, "Confirmed");

            // ── إشعار لليوزر ────────────────────────────────────────
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = order.IdUser,
                Type = "PaymentConfirmed",
                Title = "✅ Payment Confirmed",
                Message = $"Your CliQ payment for order #{order.OrderCode} has been confirmed. Your order is now being processed!",
                IsRead = false,
                RelatedEntityType = "Order",
                RelatedEntityId = idOrder,
                CreatedAt = DateTime.UtcNow
            });

            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> SettingsWallet()
        {
            if (!IsStoreOwner()) return RedirectToAction("Login", "Account");
            var store = await GetCurrentStore();
            if (store == null) return RedirectToAction("Login", "Account");
            await SetSidebarDataAsync(store);

            ViewData["Title"] = "Store Wallet";
            ViewData["ActiveNav"] = "Settings";
            ViewData["ActiveSettings"] = "Wallet";
            ViewData["BreadSection"] = "Settings";
            ViewData["BreadPage"] = "Store Wallet";

            var wallet = await _storeWalletRepo.GetOrCreateAsync(store.IdStore);
            var txs = await _storeWalletRepo.GetTransactionsAsync(wallet.IdStoreWallet);

            ViewBag.Balance = wallet.Balance;
            ViewBag.Transactions = txs;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> WalletTopUp(decimal amount, string? notes)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            if (amount <= 0)
                return Json(new { success = false, message = "Invalid amount." });

            var wallet = await _storeWalletRepo.GetOrCreateAsync(store.IdStore);

            await _storeWalletRepo.AddBalanceAsync(wallet.IdStoreWallet, amount);
            await _storeWalletRepo.AddTransactionAsync(new StoreWalletTransaction
            {
                IdStoreWallet = wallet.IdStoreWallet,
                Type = "TopUp",
                Amount = amount,
                Notes = string.IsNullOrWhiteSpace(notes) ? "Manual top-up" : notes.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            var newBalance = wallet.Balance + amount;
            return Json(new { success = true, newBalance });
        }

        [HttpPost]
        public async Task<IActionResult> WalletWithdraw(decimal amount, string? notes)
        {
            if (!IsStoreOwner()) return Json(new { success = false });
            var store = await GetCurrentStore();
            if (store == null) return Json(new { success = false });

            if (amount <= 0)
                return Json(new { success = false, message = "Invalid amount." });

            var wallet = await _storeWalletRepo.GetOrCreateAsync(store.IdStore);

            if (wallet.Balance < amount)
                return Json(new { success = false, message = $"Insufficient balance. Available: {wallet.Balance:F3} JD" });

            await _storeWalletRepo.DeductBalanceAsync(wallet.IdStoreWallet, amount);
            await _storeWalletRepo.AddTransactionAsync(new StoreWalletTransaction
            {
                IdStoreWallet = wallet.IdStoreWallet,
                Type = "Withdrawal",
                Amount = -amount,
                Notes = string.IsNullOrWhiteSpace(notes) ? "Manual withdrawal" : notes.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            var newBalance = wallet.Balance - amount;
            return Json(new { success = true, newBalance });
        }


    }
}