using Waffy.Models;

namespace Waffy.Models.ViewModels
{
    public class WalletViewModel
    {
        public User User { get; set; } = null!;
        public Wallet? Wallet { get; set; }
        public decimal Balance => Wallet?.Balance ?? 0;
        public List<WalletTransaction> Transactions { get; set; } = new();

        // ✅ إحصائيات
        public decimal TotalIn => Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        public decimal TotalOut => Transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));

        // ✅ للـ Rewards — مكافآت الـ finder
        public List<FoundPetClaim> FinderRewards { get; set; } = new();

        // ✅ للـ Timeline
        public List<TimelineEvent> TimelineEvents { get; set; } = new();
        public List<Donation> Donations { get; set; } = new();
        public List<DangerousAnimalReport> DangerReports { get; set; } = new();

        public List<Complaint> Complaints { get; set; } = new();
    }

    public class TimelineEvent
    {
        public string Icon { get; set; } = "🐾";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public string Color { get; set; } = "#7bb33a";
    }
}