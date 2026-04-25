using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IDonationRepository _donationRepo;

        public PaymentController(
            IWalletRepository walletRepo,
            IDonationRepository donationRepo)
        {
            _walletRepo = walletRepo;
            _donationRepo = donationRepo;
        }

        public IActionResult Cart() => View();
        public IActionResult Check() => View();

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

            ViewBag.WalletBalance = balance;
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
            string? targetType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var refCode = $"WAF-DON-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            // ── تبرع مالي: يُخصم من الـ Wallet ──
            if (donationType == "Money")
            {
                if (amount == null || amount <= 0)
                {
                    TempData["DonateError"] = "Please enter a valid donation amount.";
                    return RedirectToAction("Donate");
                }

                var wallet = await _walletRepo.GetByUserIdAsync(userId.Value);

                if (wallet == null || wallet.Balance < amount.Value)
                {
                    TempData["DonateError"] =
                        $"Insufficient wallet balance. Your balance: {wallet?.Balance ?? 0:F2} JOD.";
                    return RedirectToAction("Donate");
                }

                // ✅ حفظ التبرع
                var donationId = await _donationRepo.CreateAsync(new Donation
                {
                    IdUser = userId.Value,
                    DonationType = "Money",
                    Amount = amount,
                    Description = description,
                    TargetType = string.IsNullOrEmpty(targetType) ? "General" : targetType,
                    ReferenceCode = refCode,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                });

                // ✅ خصم من الـ Wallet
                await _walletRepo.DeductBalanceAsync(wallet.IdWallet, amount.Value);

                // ✅ تسجيل الـ Transaction (مبلغ سالب)
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
                    $"💚 Thank you! {amount:F2} JOD donated. Reference: {refCode}";
            }
            else
            {
                // ── تبرع عيني (Food / Medicine / Supplies) ──
                if (quantity == null || quantity <= 0) quantity = 1;

                await _donationRepo.CreateAsync(new Donation
                {
                    IdUser = userId.Value,
                    DonationType = donationType,
                    Quantity = quantity,
                    Unit = unit,
                    Description = description,
                    TargetType = string.IsNullOrEmpty(targetType) ? "General" : targetType,
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