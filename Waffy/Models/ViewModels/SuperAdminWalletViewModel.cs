using Waffy.Models;

namespace Waffy.Models.ViewModels
{
    public class SuperAdminWalletViewModel
    {
        public Wallet? Wallet { get; set; }

        public List<WalletTransaction> Transactions { get; set; } = new();

        public decimal CurrentBalance { get; set; }

        public decimal TotalDepositsCollected { get; set; }

        public decimal TotalDepositsReturned { get; set; }

        public decimal TotalDepositsRetained { get; set; }

        public int TotalTransactions { get; set; }

        public decimal TotalLostPetFees { get; set; }
    }
}