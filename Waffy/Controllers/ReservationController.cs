using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IAdoptionRepository _adoptionRepo;
        private readonly IPetRepository _petRepo;

        public ReservationController(
            IReservationRepository reservationRepo,
            IAdoptionRepository adoptionRepo,
            IPetRepository petRepo)
        {
            _reservationRepo = reservationRepo;
            _adoptionRepo = adoptionRepo;
            _petRepo = petRepo;
        }

        private int? GetCurrentUserId() =>
            HttpContext.Session.GetInt32("UserId");

        // ════════════════════════════════════════════════════
        // RESERVE — POST من Adapt.cshtml
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int postId, string? message)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var post = await _adoptionRepo.GetByIdAsync(postId);
            if (post == null || post.AdoptionStatus != "Available")
            {
                TempData["ReserveError"] = "This pet is no longer available.";
                return RedirectToAction("Adapt", "Pet");
            }

            // ✅ لا يحجز صاحب الإعلان نفسه
            if (post.PostedById == userId.Value)
            {
                TempData["ReserveError"] = "You cannot reserve your own pet.";
                return RedirectToAction("Adapt", "Pet");
            }

            // ✅ لا يحجز مرتين
            if (await _reservationRepo.HasPendingAsync(postId, userId.Value))
            {
                TempData["ReserveError"] = "You already have a pending request for this pet.";
                return RedirectToAction("Adapt", "Pet");
            }

            // ✅ أضف الطلب
            var reservation = new AdoptionReservation
            {
                IdPost = postId,
                IdRequester = userId.Value,
                Status = "Pending",
                Message = message?[..Math.Min(message?.Length ?? 0, 500)],
                CreatedAt = DateTime.UtcNow
            };

            await _reservationRepo.AddAsync(reservation);

            // ✅ غيّر حالة الإعلان
            await _adoptionRepo.UpdateStatusAsync(postId, "Reserved");

            TempData["ReserveSuccess"] = "Your reservation request has been sent!";
            return RedirectToAction("Adapt", "Pet");
        }

        // ════════════════════════════════════════════════════
        // APPROVE — POST من Profile
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int reservationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var reservation = await _reservationRepo.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                TempData["AdoptionError"] = "Reservation not found.";
                return RedirectToAction("Adoption_Ads", "Profile");
            }

            var post = await _adoptionRepo.GetByIdAsync(reservation.IdPost);
            if (post == null || post.PostedById != userId.Value)
            {
                TempData["AdoptionError"] = "Unauthorized.";
                return RedirectToAction("Adoption_Ads", "Profile");
            }

            // ✅ 1 — نقل ملكية الحيوان
            // User posts have a Pet record → transfer ownership
            // Store posts (IdPet = null) → skip, no pet record to transfer
            if (post.IdPet.HasValue)
                await _petRepo.TransferOwnershipAsync(post.IdPet.Value, reservation.IdRequester);
            // ✅ 2 — أغلق الإعلان
            await _adoptionRepo.ClosePostAsync(post.IdPost);

            // ✅ 3 — وافق على الطلب
            await _reservationRepo.ApproveAsync(reservationId);


            // ✅ 4 — ارفض باقي الطلبات على نفس الإعلان
            var others = (await _reservationRepo.GetByPostIdAsync(post.IdPost))
                .Where(r => r.IdReservation != reservationId && r.Status == "Pending");
            foreach (var other in others)
                await _reservationRepo.RejectAsync(other.IdReservation);

            TempData["AdoptionSuccess"] = "✅ Ownership transferred successfully!";
            return RedirectToAction("Adoption_Ads", "Profile");
        }

        // ════════════════════════════════════════════════════
        // REJECT — POST من Profile
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int reservationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var reservation = await _reservationRepo.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                TempData["AdoptionError"] = "Reservation not found.";
                return RedirectToAction("Adoption_Ads", "Profile");
            }

            var post = await _adoptionRepo.GetByIdAsync(reservation.IdPost);
            if (post == null || post.PostedById != userId.Value)
            {
                TempData["AdoptionError"] = "Unauthorized.";
                return RedirectToAction("Adoption_Ads", "Profile");
            }

            await _reservationRepo.RejectAsync(reservationId);

            // ✅ لو لا يوجد pending أخرى — أعد الإعلان Available
            var stillPending = (await _reservationRepo.GetByPostIdAsync(post.IdPost))
                .Any(r => r.Status == "Pending" && r.IdReservation != reservationId);

            if (!stillPending)
                await _adoptionRepo.UpdateStatusAsync(post.IdPost, "Available");

            TempData["AdoptionSuccess"] = "Request rejected.";
            return RedirectToAction("Adoption_Ads", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int reservationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var reservation = await _reservationRepo.GetByIdAsync(reservationId);
            if (reservation == null || reservation.IdRequester != userId.Value)
            {
                TempData["AdoptionError"] = "Reservation not found.";
                return RedirectToAction("Adoption_Ads", "Profile");
            }

            if (reservation.Status != "Pending")
            {
                TempData["AdoptionError"] = "Only pending requests can be cancelled.";
                return RedirectToAction("Adoption_Ads", "Profile");
            }

            await _reservationRepo.CancelAsync(reservationId);

            TempData["AdoptionSuccess"] = "Your reservation request has been cancelled.";
            return RedirectToAction("Adoption_Ads", "Profile");
        }
    }
}