using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IDonationRepository _donationRepo;
        private readonly IDonationAnimalRepository _donationAnimalRepo;

        public PaymentController(
            IWalletRepository walletRepo,
            IDonationRepository donationRepo,
            IDonationAnimalRepository donationAnimalRepo)   
        {
            _walletRepo = walletRepo;
            _donationRepo = donationRepo;
            _donationAnimalRepo = donationAnimalRepo;
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