using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

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
        public PaymentController(
     IWalletRepository walletRepo,
     IDonationRepository donationRepo,
     IDonationAnimalRepository donationAnimalRepo,
     ICartRepository cartRepo,
     IProductRepository productRepo,
     IOrderRepository orderRepo)   // ← أضف
        {
            _walletRepo = walletRepo;
            _donationRepo = donationRepo;
            _donationAnimalRepo = donationAnimalRepo;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo; // ← أضف
        }
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);

            // ✅ احذف أي منتج خلص مخزونه تلقائياً
            await _cartRepo.RemoveOutOfStockItemsAsync(cart.IdCart);

            var items = (await _cartRepo.GetCartItemsAsync(cart.IdCart)).ToList();
            var subtotal = items.Sum(i => i.Quantity * i.EffectivePrice);

            ViewBag.CartId = cart.IdCart;
            ViewBag.Subtotal = subtotal;
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

            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);
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
                await _cartRepo.RemoveItemAsync(itemId);
            else
                await _cartRepo.UpdateQuantityAsync(itemId, quantity);

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

            if (!storeItems.Any())
                return RedirectToAction("Cart");

            var vm = new CheckoutViewModel
            {
                IdStore = storeId,
                StoreName = storeItems.First().StoreName,
                Items = storeItems,
                DeliveryFee = 2m,
                // Pre-fill من Session لو موجود
                RecipientName = HttpContext.Session.GetString("FullName") ?? "",
                RecipientPhone = HttpContext.Session.GetString("Phone") ?? "",
                DeliveryAddress = HttpContext.Session.GetString("Address") ?? "",
            };

            if (TempData["CheckoutError"] is string err)
                ViewBag.Error = err;

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
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(recipientName) ||
                string.IsNullOrWhiteSpace(recipientPhone) ||
                string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["CheckoutError"] = "Please fill in all required fields.";
                return RedirectToAction("Check", new { storeId = idStore });
            }

            var cart = await _cartRepo.GetOrCreateCartAsync(userId.Value);
            var allItems = (await _cartRepo.GetCartItemsAsync(cart.IdCart)).ToList();
            var storeItems = allItems.Where(i => i.IdStore == idStore).ToList();

            if (!storeItems.Any())
                return RedirectToAction("Cart");

            // ✅ تحقق من الـ Stock قبل الحفظ
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
                // احذفهم من الكارت
                await _cartRepo.RemoveOutOfStockItemsAsync(cart.IdCart);
                return RedirectToAction("Check", new { storeId = idStore });
            }

            // ── إنشاء الطلب ──────────────────────────────────────────
            var orderCode = $"WAF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var deliveryFee = 2m;

            var orderId = await _orderRepo.CreateAsync(new Order
            {
                IdUser = userId.Value,
                IdStore = idStore,
                OrderCode = orderCode,
                Status = "Pending",
                PaymentMethod = paymentMethod,
                PaymentStatus = "Pending",
                RecipientName = recipientName.Trim(),
                RecipientPhone = recipientPhone.Trim(),
                DeliveryAddress = deliveryAddress.Trim(),
                DeliveryFee = deliveryFee,
                Notes = notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            // ── إضافة العناصر وخصم الـ Stock ─────────────────────────
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

                // ✅ خصم الكمية من المنتج
                await _productRepo.DeductStockAsync(item.IdProduct, item.Quantity);
            }

            // ── تفريغ عناصر هذا المتجر من الكارت ────────────────────
            await _cartRepo.ClearCartByStoreAsync(cart.IdCart, idStore);

            TempData["OrderCode"] = orderCode;
            TempData["StoreName"] = storeItems.First().StoreName;
            TempData["OrderTotal"] =
                (storeItems.Sum(i => i.EffectivePrice * i.Quantity) + deliveryFee).ToString("F3");

            return RedirectToAction("OrderSuccess");
        }

        [HttpGet]
        public IActionResult OrderSuccess()
        {
            if (TempData["OrderCode"] is not string code)
                return RedirectToAction("Cart");

            ViewBag.OrderCode = code;
            ViewBag.StoreName = TempData["StoreName"];
            ViewBag.OrderTotal = TempData["OrderTotal"];
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


    }
}