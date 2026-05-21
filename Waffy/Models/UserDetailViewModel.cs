using Waffy.Models.DTOs;
namespace Waffy.Models.ViewModels
{
    public class UserDetailViewModel
    {
        public UserAdminDto User { get; set; } = null!;

        // ── Lists — تُملأ من الـ repos ──────────────────
        public List<Pet> Pets { get; set; } = new();
        public List<LostPetReport> LostReports { get; set; } = new();
        public List<AdoptionPost> AdoptionPosts { get; set; } = new();
        public List<RewardLogDto> SurpriseRewards { get; set; } = new(); 
        public List<WalletTransaction> Transactions { get; set; } = new();  
        public Wallet? Wallet { get; set; }
        public List<Order> Orders { get; set; } = new();
        public int TotalFlagsCount { get; set; }
        public List<Complaint> Complaints { get; set; } = new();
        public int ComplaintsCount { get; set; }
        public List<Notification> MessageHistory { get; set; } = new();

        // ── Counts — Mini Stats ──────────────────────────
        public int PetsCount { get; set; }
        public int AppointmentsCount { get; set; }
        public int OrdersCount { get; set; }
        public int LostReportsCount { get; set; }
        public int AdoptionCount { get; set; }
        public int CouponsCount { get; set; }

        // ── PRG return params ────────────────────────────
        public string? ReturnSearch { get; set; }
        public string? ReturnStatus { get; set; }
        public string? ReturnRole { get; set; }
        public int ReturnPage { get; set; } = 1;
    }
}
