using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class FoundPetClaimController : Controller
    {
        private readonly IFoundPetClaimRepository _claimRepo;
        private readonly ILostPetRepository _lostRepo;
        private readonly IConfiguration _config;
        private readonly ISurpriseRewardRepository _surpriseRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly IUserRepository _userRepo;

        public FoundPetClaimController(
            IFoundPetClaimRepository claimRepo,
            ILostPetRepository lostRepo,
            IConfiguration config,
            ISurpriseRewardRepository surpriseRepo,
INotificationRepository notificationRepo,
            IUserRepository userRepo
            )
        {
            _claimRepo = claimRepo;
            _lostRepo = lostRepo;
            _config = config;
            _surpriseRepo = surpriseRepo;
            _notifRepo = notificationRepo;
            _userRepo = userRepo;
        }

        private int? GetCurrentUserId() =>
            HttpContext.Session.GetInt32("UserId");

        // ════════════════════════════════════════════════════
        // SUBMIT — POST AJAX من lost.js
        // ════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> Submit(
      int reportId,
      string secretAnswer)
        {
            var userId = GetCurrentUserId();

            // ✅ المستخدم يجب أن يكون مسجل دخول
            if (userId == null)
                return Json(new { success = false, message = "Please log in to report a found pet." });

            var report = await _lostRepo.GetByIdAsync(reportId);
            if (report == null)
                return Json(new { success = false, message = "Report not found." });

            // ✅ صاحب الحيوان لا يستطيع أن يكون finder
            if (userId.Value == report.IdUser)
                return Json(new { success = false, message = "You cannot claim your own pet." });

            // ✅ جلب بيانات المستخدم من DB
            var user = await _userRepo.GetByIdAsync(userId.Value);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            // ✅ تحقق من السؤال السري بـ SHA256
            var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(secretAnswer.Trim().ToLower())));
            var isVerified = string.Equals(
                hash, report.SecretAnswerHash,
                StringComparison.OrdinalIgnoreCase);

            var claim = new FoundPetClaim
            {
                IdLostReport = reportId,
                IdUser = userId.Value,
                FinderName = user.FullName,     // ← من DB
                FinderPhone = user.Phone ?? "",  // ← من DB
                Description = "Submitted via Lost page",
                IsVerified = isVerified,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _claimRepo.AddAsync(claim);

            return Json(new
            {
                success = true,
                verified = isVerified,
                phone = user.Phone ?? "—",   // ✅ نرسله للـ JS لعرضه في Step 2
                message = isVerified
                    ? "✅ Correct! The owner will contact you soon."
                    : "⚠️ Wrong answer, but your info is saved. Owner may still contact you."
            });
        }


        // ════════════════════════════════════════════════════
        // CONFIRM FOUND — POST من Lost_Reports
        // ════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmFound(int claimId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var claim = await _claimRepo.GetByIdAsync(claimId);
            var report = claim != null ? await _lostRepo.GetByIdAsync(claim.IdLostReport) : null;

            if (claim == null || report == null || report.IdUser != userId.Value)
            {
                TempData["LostError"] = "Unauthorized or not found.";
                return RedirectToAction("Lost_Reports", "Profile");
            }

            await _claimRepo.ConfirmAsync(claimId);
            await _lostRepo.CloseReportAsync(report.IdLostReport);

            var others = await _claimRepo.GetByReportIdAsync(report.IdLostReport);
            foreach (var other in others.Where(c => c.IdClaim != claimId && c.Status == "Pending"))
                await _claimRepo.RejectAsync(other.IdClaim);

            string successMsg;

            // مكافأة من المالك
            if (report.RewardAmount > 0 && claim.IdUser.HasValue)
            {
                await AddRewardToWalletAsync(claim.IdUser.Value, report.RewardAmount.Value);
                await UpdateRewardStatusAsync(report.IdLostReport, "Released");

                successMsg = $"🎉 Pet returned! {report.RewardAmount} JOD sent to finder's wallet.";
            }
            // مكافأة مفاجأة من Waffy
            else if (claim.IdUser.HasValue)
            {
                var surprise = await _surpriseRepo.GetAvailableAsync();
                if (surprise != null)
                {
                    await _surpriseRepo.MarkAsUsedAsync(surprise.IdReward, claim.IdUser.Value);

                    // إضافة رصيد للمحفظة لو النوع WalletCredit
                    if (surprise.RewardType == "WalletCredit" && surprise.Value.HasValue)
                        await AddRewardToWalletAsync(claim.IdUser.Value, surprise.Value.Value);

                    await _notifRepo.AddAsync(new Notification
                    {
                        IdUser = claim.IdUser.Value,
                        Type = "Reward",
                        Title = "🎁 Surprise! You got a reward from Waffy",
                        Message = surprise.RewardType == "WalletCredit"
                            ? $"🎉 {surprise.Title} — {surprise.Value} JOD added to your wallet!"
                            : $"🎉 {surprise.Title}" +
                              (surprise.CouponCode != null ? $" — Code: {surprise.CouponCode}" : "") +
                              (surprise.PartnerName != null ? $" at {surprise.PartnerName}" : ""),
                        IsRead = false,
                        RelatedEntityType = "FoundPetClaim",
                        RelatedEntityId = claimId,
                        CreatedAt = DateTime.UtcNow
                    });

                    successMsg = "✅ Pet returned! Finder received a Waffy surprise reward.";
                }
                else
                {
                    successMsg = "✅ Pet returned successfully!";
                }
            }
            else
            {
                successMsg = "✅ Pet returned successfully!";
            }

            TempData["LostSuccess"] = successMsg;
            return RedirectToAction("Lost_Reports", "Profile");
        }

        // ════════════════════════════════════════════════════
        // REJECT CLAIM — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int claimId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var claim = await _claimRepo.GetByIdAsync(claimId);
            var report = claim != null ? await _lostRepo.GetByIdAsync(claim.IdLostReport) : null;

            if (claim == null || report == null || report.IdUser != userId.Value)
            {
                TempData["LostError"] = "Unauthorized.";
                return RedirectToAction("Lost_Reports", "Profile");
            }

            await _claimRepo.RejectAsync(claimId);
            TempData["LostSuccess"] = "Claim rejected.";
            return RedirectToAction("Lost_Reports", "Profile");
        }

        // ════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════
        private async Task AddRewardToWalletAsync(int userId, decimal amount)
        {
            using var conn = new SqlConnection(
                _config.GetConnectionString("WaffyConnection"));

            // ✅ لو wallet موجودة — أضف
            var walletId = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT IdWallet FROM Wallet WHERE UserId = @UserId",
                new { UserId = userId });

            if (walletId.HasValue)
            {
                await conn.ExecuteAsync(
                    "UPDATE Wallet SET Balance = Balance + @Amount, UpdatedAt = GETUTCDATE() WHERE IdWallet = @WalletId",
                    new { Amount = amount, WalletId = walletId.Value });
            }
            else
            {
                // ✅ أنشئ wallet جديدة بـ MAX + 1
                await conn.ExecuteAsync(@"
                    INSERT INTO Wallet (IdWallet, UserId, Balance, Status, CreatedAt, UpdatedAt)
                    SELECT ISNULL(MAX(IdWallet), 0) + 1, @UserId, @Amount, 'Active', GETUTCDATE(), GETUTCDATE()
                    FROM Wallet",
                    new { UserId = userId, Amount = amount });
            }
        }

        private async Task UpdateRewardStatusAsync(int reportId, string status)
        {
            using var conn = new SqlConnection(
                _config.GetConnectionString("WaffyConnection"));
            await conn.ExecuteAsync(
                "UPDATE LostPetReport SET RewardStatus = @Status WHERE IdLostReport = @ReportId",
                new { Status = status, ReportId = reportId });
        }
    }
}