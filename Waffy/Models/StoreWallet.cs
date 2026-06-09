// ══════════════════════════════════════════════════════════════
// Models/StoreWallet.cs
// ══════════════════════════════════════════════════════════════
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Models
{
    public class StoreWallet
    {
        public int IdStoreWallet { get; set; }
        public int IdStore { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class StoreWalletTransaction
    {
        public int IdTx { get; set; }
        public int IdStoreWallet { get; set; }
        public string Type { get; set; } = "";   // TopUp / Refund / Withdrawal
        public decimal Amount { get; set; }         // + دخول / - خروج
        public string? Notes { get; set; }
        public int? RelatedOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


// ═════════════════════════════

