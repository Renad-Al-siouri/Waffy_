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

        public FoundPetClaimController(
            IFoundPetClaimRepository claimRepo,
            ILostPetRepository lostRepo,
            IConfiguration config)
        {
            _claimRepo = claimRepo;
            _lostRepo = lostRepo;
            _config = config;
        }

        private int? GetCurrentUserId() =>
            HttpContext.Session.GetInt32("UserId");

        // ════════════════════════════════════════════════════
        // SUBMIT — POST AJAX من lost.js
        // ════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> Submit(
            int reportId,
            string finderName,
            string finderPhone,
            string secretAnswer)
        {
            var report = await _lostRepo.GetByIdAsync(reportId);
            if (report == null)
                return Json(new { success = false, message = "Report not found." });

            var userId = GetCurrentUserId();

            // ✅ صاحب الحيوان لا يستطيع أن يكون finder
            if (userId.HasValue && userId.Value == report.IdUser)
                return Json(new { success = false, message = "You cannot claim your own pet." });

            // ✅ تحقق من السؤال السري بـ SHA256
            var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(secretAnswer.Trim().ToLower())));
            var isVerified = string.Equals(
                hash, report.SecretAnswerHash,
                StringComparison.OrdinalIgnoreCase);

            var claim = new FoundPetClaim
            {
                IdLostReport = reportId,
                IdUser = userId,
                FinderName = finderName.Trim(),
                FinderPhone = finderPhone.Trim(),
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
                message = isVerified
                    ? "✅ Correct! The owner will contact you soon."
                    : "⚠️ Wrong answer, but your info is saved. Owner may still contact you."
            });
        }

        // ════════════════════════════════════════════════════
        // CONFIRM FOUND — POST من Lost_Reports
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // ✅ 1 — تأكيد الـ claim
            await _claimRepo.ConfirmAsync(claimId);

            // ✅ 2 — أغلق التقرير
            await _lostRepo.CloseReportAsync(report.IdLostReport);

            // ✅ 3 — أرفض باقي الـ claims
            var others = await _claimRepo.GetByReportIdAsync(report.IdLostReport);
            foreach (var other in others.Where(c => c.IdClaim != claimId && c.Status == "Pending"))
                await _claimRepo.RejectAsync(other.IdClaim);

            // ✅ 4 — أضف المكافأة للمحفظة لو كانت مالية
            if (report.RewardAmount > 0 && claim.IdUser.HasValue)
            {
                await AddRewardToWalletAsync(claim.IdUser.Value, report.RewardAmount.Value);
                await UpdateRewardStatusAsync(report.IdLostReport, "Released");
            }

            TempData["LostSuccess"] = report.RewardAmount > 0
                ? $"🎉 Pet returned! {report.RewardAmount} JOD reward sent to finder's wallet."
                : "✅ Pet returned successfully!";

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