// ══════════════════════════════════════════════════════════
// أضف هذا في نهاية ClinicAdminViewModels.cs
// ══════════════════════════════════════════════════════════

using Waffy.Models;

namespace Waffy.Models.ViewModels
{
    public class ClinicAdoptionViewModel
    {
        public List<AdoptionPost> Posts { get; set; } = new();

        // Reservations لكل post — Key = IdPost
        public Dictionary<int, List<AdoptionReservation>> Reservations { get; set; } = new();

        // ✅ أول صورة لكل post — Key = IdPost
        public Dictionary<int, string> PostImages { get; set; } = new();
        public string StatusFilter { get; set; } = "All";

        // KPI counts
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int ClosedCount { get; set; }
        public int RejectedCount { get; set; }

        // Pending reservations count — للـ badge
        public int PendingReservationsCount =>
            Reservations.Values
                        .SelectMany(r => r)
                        .Count(r => r.Status == "Pending");
    }
}