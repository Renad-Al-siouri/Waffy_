using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;
using static System.Formats.Asn1.AsnWriter;

namespace Waffy.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IDonationRepository _donationRepo;
        private readonly IDonationAnimalRepository _donationAnimalRepo;
        private readonly ICartRepository _cartRepo;          // ← أضف
        private readonly IProductRepository _productRepo;    // ← أضف
        private readonly IOrderRepository _orderRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly IUserRepository _userRepo;
        private readonly IStoreWalletRepository _storeWalletRepo;
        public PaymentController(
     IWalletRepository walletRepo,
     IDonationRepository donationRepo,
     IDonationAnimalRepository donationAnimalRepo,
     ICartRepository cartRepo,
     IProductRepository productRepo,
     IOrderRepository orderRepo,
     INotificationRepository notifRepo,
     IUserRepository userRepo,
     IStoreWalletRepository storeWalletRepo,
             IStoreRepository storeRepo)   
        {
            _walletRepo = walletRepo;
            _donationRepo = donationRepo;
            _donationAnimalRepo = donationAnimalRepo;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _storeRepo = storeRepo;
            _notifRepo = notifRepo;
            _userRepo = userRepo;
            _storeWalletRepo = storeWalletRepo;
        }
        [HttpGet]
        // Cart action — أضف جلب الديليفري في آخره قبل return View
        public async Task<IActionResult> Cart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);
            await _cartRepo.RemoveOutOfStockItemsAsync(cart.IdCart);
            var items = (await _cartRepo.GetCartItemsAsync(cart.IdCart)).ToList();
            var subtotal = items.Sum(i => i.Quantity * i.EffectivePrice);

            // ── جلب سعر التوصيل لكل متجر ──
            var deliveryFees = new Dictionary<int, decimal>();
            foreach (var storeId in items.Select(i => i.IdStore).Distinct())
            {
                var s = await _storeRepo.GetByIdAsync(storeId);
                deliveryFees[storeId] = (s?.DeliveryAvailable == true) ? (s.DeliveryFee ?? 0m) : 0m;
            }

            ViewBag.CartId = cart.IdCart;
            ViewBag.Subtotal = subtotal;
            ViewBag.DeliveryFees = deliveryFees;   // ← جديد
            return View(items);
        }
        // ══ Add to Cart (AJAX) ════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int storeId, int quantity = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Please login first." });
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null || product.StockQuantity <= 0)
                return Json(new { success = false, message = "Out of stock." });

            if (quantity > product.StockQuantity)
                return Json(new { success = false, message = $"Only {product.StockQuantity} unit(s) available." });

            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);

            // تحقق من الكمية الموجودة بالكارت مسبقاً
            var currentInCart = await _cartRepo.GetItemQuantityAsync(cart.IdCart, productId);
            if (currentInCart + quantity > product.StockQuantity)
            {
                var canAdd = product.StockQuantity - currentInCart;
                if (canAdd <= 0)
                    return Json(new { success = false, message = $"Max quantity ({product.StockQuantity}) already in cart." });
                return Json(new { success = false, message = $"Only {canAdd} more unit(s) can be added." });
            }

            await _cartRepo.AddOrUpdateItemAsync(cart.IdCart, productId, storeId, quantity);
            var count = await _cartRepo.GetCartCountAsync(cart.IdCart);

            return Json(new { success = true, cartCount = count });
        }

        // ══ Update Quantity (AJAX) ════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            if (quantity <= 0)
            {
                await _cartRepo.RemoveItemAsync(itemId);
            }
            else
            {
                var item = await _cartRepo.GetItemByIdAsync(itemId);
                if (item != null)
                {
                    var product = await _productRepo.GetByIdAsync(item.IdProduct);
                    if (product != null && quantity > product.StockQuantity)
                        return Json(new
                        {
                            success = false,
                            message = $"Only {product.StockQuantity} unit(s) available.",
                            maxQty = product.StockQuantity
                        });
                }
                await _cartRepo.UpdateQuantityAsync(itemId, quantity);
            }
            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);
            var items = (await _cartRepo.GetCartItemsAsync(cart.IdCart)).ToList();
            var subtotal = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice));

            return Json(new
            {
                success = true,
                subtotal = subtotal.ToString("F3"),
                cartCount = items.Sum(i => i.Quantity),
                empty = !items.Any()
            });
        }

        // ══ Remove Item (AJAX) ════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            await _cartRepo.RemoveItemAsync(itemId);

            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);
            var items = (await _cartRepo.GetCartItemsAsync(cart.IdCart)).ToList();
            var subtotal = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice));

            return Json(new
            {
                success = true,
                subtotal = subtotal.ToString("F3"),
                cartCount = items.Sum(i => i.Quantity),
                empty = !items.Any()
            });
        }
        [HttpGet]
        public async Task<IActionResult> Check(int storeId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);
            var allItems = (await _cartRepo.GetCartItemsAsync(cart.IdCart)).ToList();
            var storeItems = allItems.Where(i => i.IdStore == storeId).ToList();

            if (!storeItems.Any()) return RedirectToAction("Cart");

            // جلب سعر التوصيل من المتجر
            var store = await _storeRepo.GetByIdAsync(storeId);
            var deliveryFee = (store?.DeliveryAvailable == true) ? (store.DeliveryFee ?? 0m) : 0m;

            // جلب بيانات المستخدم من DB
            var user = await _userRepo.GetByIdAsync(userId.Value);

            var vm = new CheckoutViewModel
            {
                IdStore = storeId,
                StoreName = storeItems.First().StoreName,
                Items = storeItems,
                DeliveryFee = deliveryFee,
                RecipientName = user?.FullName ?? HttpContext.Session.GetString("FullName") ?? "",
                RecipientPhone = user?.Phone ?? HttpContext.Session.GetString("Phone") ?? "",
                DeliveryAddress = (user != null && !string.IsNullOrEmpty(user.AddressDetail))
                                  ? $"{user.City}, {user.Area}, {user.AddressDetail}".Trim(',', ' ')
                                  : HttpContext.Session.GetString("Address") ?? "",
            };

            if (TempData["CheckoutError"] is string err)
                ViewBag.Error = err;

            var wallet = await _walletRepo.GetByUserIdAsync(userId.Value);
            ViewBag.WalletBalance = wallet?.Balance ?? 0m;
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrder(
          int idStore,
          string recipientName,
          string recipientPhone,
          string deliveryAddress,
          string? notes,
          string paymentMethod)
        {
            // ── 1. التحقق من تسجيل الدخول ──────────────────────────
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // ── 2. التحقق من الحقول المطلوبة ───────────────────────
            if (string.IsNullOrWhiteSpace(recipientName) ||
                string.IsNullOrWhiteSpace(recipientPhone) ||
                string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["CheckoutError"] = "Please fill in all required fields.";
                return RedirectToAction("Check", new { storeId = idStore });
            }

            // ── 3. جلب عناصر الكارت ────────────────────────────────
            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);
            var allItems = (await _cartRepo.GetCartItemsAsync(cart.IdCart)).ToList();
            var storeItems = allItems.Where(i => i.IdStore == idStore).ToList();

            if (!storeItems.Any())
                return RedirectToAction("Cart");

            // ── 4. جلب المتجر وسعر التوصيل ─────────────────────────
            var store = await _storeRepo.GetByIdAsync(idStore);
            var deliveryFee = (store?.DeliveryAvailable == true) ? (store.DeliveryFee ?? 0m) : 0m;
            var orderTotal = storeItems.Sum(i => i.EffectivePrice * i.Quantity) + deliveryFee;

            // ── 5. التحقق من رصيد المحفظة (إذا Wallet) ─────────────
            if (paymentMethod?.ToLower() == "wallet")
            {
                var wallet = await _walletRepo.GetByUserIdAsync(userId.Value);
                if (wallet == null || wallet.Balance < orderTotal)
                {
                    TempData["CheckoutError"] = $"Insufficient wallet balance. Your balance: {wallet?.Balance ?? 0:F3} JD, Order total: {orderTotal:F3} JD.";
                    return RedirectToAction("Check", new { storeId = idStore });
                }
            }

            // ── 6. التحقق من الـ Stock ──────────────────────────────
            var outOfStock = new List<string>();
            foreach (var item in storeItems)
            {
                var product = await _productRepo.GetByIdAsync(item.IdProduct);
                if (product == null || product.StockQuantity < item.Quantity)
                    outOfStock.Add(item.ProductName);
            }

            if (outOfStock.Any())
            {
                TempData["CheckoutError"] =
                    $"Sorry, these items are out of stock: {string.Join(", ", outOfStock)}. " +
                    "They have been removed from your cart.";
                await _cartRepo.RemoveOutOfStockItemsAsync(cart.IdCart);
                return RedirectToAction("Check", new { storeId = idStore });
            }

            // ── 7. توليد كود CliQ (إذا CliQ) ───────────────────────
            string? cliqCode = null;
            if (paymentMethod?.ToLower() == "cliq")
                cliqCode = Guid.NewGuid().ToString("N")[..8].ToUpper();

            // ── 8. إنشاء الطلب ─────────────────────────────────────
            var orderCode = $"WAF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var orderId = await _orderRepo.CreateAsync(new Order
            {
                IdUser = userId.Value,
                IdStore = idStore,
                OrderCode = orderCode,
                Status = "Pending",
                RecipientName = recipientName.Trim(),
                RecipientPhone = recipientPhone.Trim(),
                DeliveryAddress = deliveryAddress.Trim(),
                DeliveryFee = deliveryFee,
                Notes = notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentMethod?.ToLower() switch
                {
                    "wallet" => "Paid",
                    "cliq" => "PendingCliQ",
                    _ => "Pending"
                },
                CliQVerificationCode = cliqCode,
                CliQVerified = false,
            });

            // ── 9. خصم من المحفظة (إذا Wallet) ─────────────────────
            // ── 9. خصم من محفظة اليوزر + إضافة لمحفظة المتجر (إذا Wallet) ──
            if (paymentMethod?.ToLower() == "wallet")
            {
                var wallet = await _walletRepo.GetByUserIdAsync(userId.Value);
                await _walletRepo.DeductBalanceAsync(wallet!.IdWallet, orderTotal);
                await _walletRepo.AddTransactionAsync(new WalletTransaction
                {
                    IdWallet = wallet.IdWallet,
                    TransactionCode = orderCode,
                    Type = "Purchase",
                    Amount = -orderTotal,
                    PaymentMethod = "Wallet",
                    ReferenceId = orderId,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                });

                // ← إضافة للمتجر
                var storeWlt = await _storeWalletRepo.GetOrCreateAsync(idStore);
                await _storeWalletRepo.AddBalanceAsync(storeWlt.IdStoreWallet, orderTotal);
                await _storeWalletRepo.AddTransactionAsync(new StoreWalletTransaction
                {
                    IdStoreWallet = storeWlt.IdStoreWallet,
                    Type = "WalletOrderReceived",
                    Amount = orderTotal,
                    Notes = $"Payment received for order #{orderCode}",
                    RelatedOrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // ── 10. إضافة عناصر الطلب وخصم المخزون ─────────────────
            foreach (var item in storeItems)
            {
                await _orderRepo.AddItemAsync(new OrderItem
                {
                    IdOrder = orderId,
                    IdProduct = item.IdProduct,
                    ProductNameSnapshot = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SalePrice = item.SalePrice
                });

                await _productRepo.DeductStockAsync(item.IdProduct, item.Quantity);

                // إشعار نفاد المخزون
                var updatedProduct = await _productRepo.GetByIdAsync(item.IdProduct);
                if (updatedProduct != null && store != null)
                {
                    if (updatedProduct.StockQuantity == 0)
                    {
                        await _notifRepo.AddAsync(new Notification
                        {
                            IdUser = store.OwnerUserId,
                            Type = "OutOfStock",
                            Title = "⚠️ Product Out of Stock",
                            Message = $"\"{updatedProduct.Name}\" is now out of stock.",
                            IsRead = false,
                            RelatedEntityType = "Product",
                            RelatedEntityId = updatedProduct.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    else if (updatedProduct.StockQuantity <= 3)
                    {
                        await _notifRepo.AddAsync(new Notification
                        {
                            IdUser = store.OwnerUserId,
                            Type = "LowStock",
                            Title = "📦 Low Stock Alert",
                            Message = $"\"{updatedProduct.Name}\" has only {updatedProduct.StockQuantity} unit(s) left.",
                            IsRead = false,
                            RelatedEntityType = "Product",
                            RelatedEntityId = updatedProduct.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            // ── 11. تفريغ الكارت ────────────────────────────────────
            await _cartRepo.ClearCartByStoreAsync(cart.IdCart, idStore);

            // ── 12. إشعار صاحب المتجر ───────────────────────────────
            if (store != null)
            {
                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = store.OwnerUserId,
                    Type = "NewOrder",
                    Title = "🛒 New Order Received!",
                    Message = $"Order #{orderCode} from {recipientName} — {orderTotal:F3} JD via {paymentMethod}",
                    IsRead = false,
                    RelatedEntityType = "Order",
                    RelatedEntityId = orderId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // ── 13. TempData للـ Success page ───────────────────────
            TempData["OrderCode"] = orderCode;
            TempData["StoreName"] = storeItems.First().StoreName;
            TempData["OrderTotal"] = orderTotal.ToString("F3");
            TempData["CliQCode"] = cliqCode;
            TempData["CliQNumber"] = store?.CliQNumber ?? "";
            TempData["PaymentMethod"] = paymentMethod;

            return RedirectToAction("OrderSuccess");
        }
        [HttpGet]
        public IActionResult OrderSuccess()
        {
            if (TempData["OrderCode"] is not string code) return RedirectToAction("Cart");
            ViewBag.OrderCode = code;
            ViewBag.StoreName = TempData["StoreName"];
            ViewBag.OrderTotal = TempData["OrderTotal"];
            ViewBag.CliQCode = TempData["CliQCode"] as string;
            ViewBag.CliQNumber = TempData["CliQNumber"] as string;
            ViewBag.PaymentMethod = TempData["PaymentMethod"] as string;
            return View();
        }

        // ════════════════════════════════════════════════════
        // DONATE — GET: جلب رصيد الـ Wallet الحقيقي
        // ════════════════════════════════════════════════════

        public async Task<IActionResult> Donate()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            decimal balance = 0;

            if (userId.HasValue)
            {
                var wallet = await _walletRepo.GetByUserIdAsync(userId.Value);
                balance = wallet?.Balance ?? 0;
            }

            var animals = (await _donationAnimalRepo.GetActiveAsync()).ToList();

            ViewBag.WalletBalance = balance;
            ViewBag.Animals = animals;
            return View();
        }

        // ════════════════════════════════════════════════════
        // SUBMIT DONATION — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDonation(
      string donationType,
      decimal? amount,
      int? quantity,
      string? unit,
      string? description,
      string? targetType,
      int? targetId,
      string? paymentMethod) // ← جديد
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var refCode = $"WAF-DON-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            if (donationType == "Money")
            {
                if (amount == null || amount <= 0)
                {
                    TempData["DonateError"] = "Please enter a valid donation amount.";
                    return RedirectToAction("Donate");
                }

                // ✅ Wallet فقط يخصم — CliQ وCard يسجل Pending
                if (paymentMethod == "wallet")
                {
                    var wallet = await _walletRepo.GetByUserIdAsync(userId.Value);

                    if (wallet == null || wallet.Balance < amount.Value)
                    {
                        TempData["DonateError"] =
                            $"Insufficient wallet balance. Your balance: {wallet?.Balance ?? 0:F2} JOD.";
                        return RedirectToAction("Donate");
                    }

                    var donationId = await _donationRepo.CreateAsync(new Donation
                    {
                        IdUser = userId.Value,
                        DonationType = "Money",
                        Amount = amount,
                        Description = description,
                        TargetType = string.IsNullOrEmpty(targetType) ? "General" : targetType,
                        TargetId = targetId,
                        ReferenceCode = refCode,
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    });

                    await _walletRepo.DeductBalanceAsync(wallet.IdWallet, amount.Value);

                    await _walletRepo.AddTransactionAsync(new WalletTransaction
                    {
                        IdWallet = wallet.IdWallet,
                        TransactionCode = refCode,
                        Type = "Donation",
                        Amount = -amount.Value,
                        PaymentMethod = "Wallet",
                        ReferenceId = donationId,
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    });

                    TempData["DonateSuccess"] =
                        $"💚 Thank you! {amount:F2} JOD donated via Wallet. Reference: {refCode}";
                }
                else
                {
                    // CliQ أو Card — يسجل فقط بدون خصم
                    await _donationRepo.CreateAsync(new Donation
                    {
                        IdUser = userId.Value,
                        DonationType = "Money",
                        Amount = amount,
                        Description = description,
                        TargetType = string.IsNullOrEmpty(targetType) ? "General" : targetType,
                        TargetId = targetId,
                        ReferenceCode = refCode,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    });

                    var method = paymentMethod == "cliq" ? "CliQ" : "Card";
                    TempData["DonateSuccess"] =
                        $"💚 Thank you! {amount:F2} JOD donation recorded via {method}. Reference: {refCode}";
                }
            }
            else
            {
                if (quantity == null || quantity <= 0) quantity = 1;

                await _donationRepo.CreateAsync(new Donation
                {
                    IdUser = userId.Value,
                    DonationType = donationType,
                    Quantity = quantity,
                    Unit = unit,
                    Description = description,
                    TargetType = string.IsNullOrEmpty(targetType) ? "General" : targetType,
                    TargetId = targetId,
                    ReferenceCode = refCode,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });

                TempData["DonateSuccess"] =
                    $"💚 Thank you! Your {donationType} donation was recorded. Reference: {refCode}";
            }

            return RedirectToAction("Donate");
        }

        [HttpPost]
        public async Task<IActionResult> NotifyCliQSent(string orderCode)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            var order = await _orderRepo.GetByCodeAsync(orderCode);
            if (order == null || order.IdUser != userId.Value)
                return Json(new { success = false, message = "Order not found." });

            var store = await _storeRepo.GetByIdAsync(order.IdStore);
            if (store != null)
            {
                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = store.OwnerUserId,
                    Type = "CliQSent",
                    Title = "📱 Customer Sent CliQ Payment",
                    Message = $"Customer claims they sent CliQ payment for order #{orderCode}. " +
                              $"Check your CliQ app for code: {order.CliQVerificationCode}",
                    IsRead = false,
                    RelatedEntityType = "Order",
                    RelatedEntityId = order.IdOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
            return Json(new { success = true });
        }
    }
}