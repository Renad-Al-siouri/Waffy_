
// ══════════════════════════════════════════════════════════════
// Repository/StoreWalletRepository.cs
// ══════════════════════════════════════════════════════════════
using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class StoreWalletRepository : IStoreWalletRepository
    {
        private readonly string _conn;

        public StoreWalletRepository(IConfiguration cfg)
            => _conn = cfg.GetConnectionString("WaffyConnection")!;

        // ── جلب المحفظة ──────────────────────────────────────
        public async Task<StoreWallet?> GetByStoreIdAsync(int storeId)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryFirstOrDefaultAsync<StoreWallet>(
                "SELECT * FROM StoreWallet WHERE IdStore = @StoreId",
                new { StoreId = storeId });
        }

        // ── جلب أو إنشاء محفظة للمتجر ───────────────────────
        public async Task<StoreWallet> GetOrCreateAsync(int storeId)
        {
            var existing = await GetByStoreIdAsync(storeId);
            if (existing != null) return existing;

            using var conn = new SqlConnection(_conn);
            var id = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO StoreWallet (IdStore, Balance, CreatedAt, UpdatedAt)
                VALUES (@StoreId, 0, GETUTCDATE(), GETUTCDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { StoreId = storeId });

            return new StoreWallet { IdStoreWallet = id, IdStore = storeId, Balance = 0 };
        }

        // ── إضافة رصيد ───────────────────────────────────────
        public async Task AddBalanceAsync(int idStoreWallet, decimal amount)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(@"
                UPDATE StoreWallet
                SET Balance   = Balance + @Amount,
                    UpdatedAt = GETUTCDATE()
                WHERE IdStoreWallet = @Id",
                new { Amount = amount, Id = idStoreWallet });
        }

        // ── خصم رصيد ─────────────────────────────────────────
        public async Task DeductBalanceAsync(int idStoreWallet, decimal amount)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(@"
                UPDATE StoreWallet
                SET Balance   = Balance - @Amount,
                    UpdatedAt = GETUTCDATE()
                WHERE IdStoreWallet = @Id
                  AND Balance >= @Amount",
                new { Amount = amount, Id = idStoreWallet });
        }

        // ── إضافة معاملة ─────────────────────────────────────
        public async Task AddTransactionAsync(StoreWalletTransaction tx)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(@"
                INSERT INTO StoreWalletTransaction
                    (IdStoreWallet, Type, Amount, Notes, RelatedOrderId, CreatedAt)
                VALUES
                    (@IdStoreWallet, @Type, @Amount, @Notes, @RelatedOrderId, @CreatedAt)",
                tx);
        }

        // ── جلب المعاملات ────────────────────────────────────
        public async Task<IEnumerable<StoreWalletTransaction>> GetTransactionsAsync(int idStoreWallet)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<StoreWalletTransaction>(@"
                SELECT * FROM StoreWalletTransaction
                WHERE IdStoreWallet = @Id
                ORDER BY CreatedAt DESC",
                new { Id = idStoreWallet });
        }
    }
}