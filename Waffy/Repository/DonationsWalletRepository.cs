using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class DonationsWalletRepository : IDonationsWalletRepository
    {
        private readonly string _conn;
        public DonationsWalletRepository(IConfiguration config)
            => _conn = config.GetConnectionString("WaffyConnection")!;

        // ── جلب المحفظة (السجل الوحيد) ──────────────────────
        public async Task<DonationsWallet?> GetAsync()
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryFirstOrDefaultAsync<DonationsWallet>(
                "SELECT TOP 1 * FROM DonationsWallet");
        }

        // ── إضافة مبلغ وارد (تبرع) ───────────────────────────
        public async Task AddIncomingAsync(
            int idWallet, decimal amount, int idDonation, string note)
        {
            using var conn = new SqlConnection(_conn);

            // تحديث الرصيد والإجمالي
            await conn.ExecuteAsync(@"
                UPDATE DonationsWallet
                SET Balance       = Balance       + @Amount,
                    TotalReceived = TotalReceived + @Amount,
                    UpdatedAt     = GETUTCDATE()
                WHERE IdWallet = @IdWallet",
                new { Amount = amount, IdWallet = idWallet });

            // تسجيل المعاملة
            await conn.ExecuteAsync(@"
                INSERT INTO DonationsWalletTransaction
                    (IdWallet, Type, Amount, IdDonation, Note, CreatedAt)
                VALUES
                    (@IdWallet, 'Incoming', @Amount, @IdDonation, @Note, GETUTCDATE())",
                new { IdWallet = idWallet, Amount = amount, IdDonation = idDonation, Note = note });
        }

        // ── توزيع مبلغ على حيوان ─────────────────────────────
        public async Task AddDistributionAsync(
            int idWallet, decimal amount, int idAnimal, string note)
        {
            using var conn = new SqlConnection(_conn);

            // تحديث رصيد المحفظة
            await conn.ExecuteAsync(@"
                UPDATE DonationsWallet
                SET Balance          = Balance          - @Amount,
                    TotalDistributed = TotalDistributed + @Amount,
                    UpdatedAt        = GETUTCDATE()
                WHERE IdWallet = @IdWallet",
                new { Amount = amount, IdWallet = idWallet });

            // تحديث CollectedAmount في الحيوان
            await conn.ExecuteAsync(@"
                UPDATE DonationAnimal
                SET CollectedAmount = CollectedAmount + @Amount
                WHERE Id = @IdAnimal",
                new { Amount = amount, IdAnimal = idAnimal });

            // تسجيل المعاملة
            await conn.ExecuteAsync(@"
                INSERT INTO DonationsWalletTransaction
                    (IdWallet, Type, Amount, IdAnimal, Note, CreatedAt)
                VALUES
                    (@IdWallet, 'Distribution', @Amount, @IdAnimal, @Note, GETUTCDATE())",
                new { IdWallet = idWallet, Amount = amount, IdAnimal = idAnimal, Note = note });
        }

        // ── جلب كل المعاملات مع اسم الحيوان ──────────────────
        public async Task<List<DonationsWalletTransaction>> GetTransactionsAsync()
        {
            using var conn = new SqlConnection(_conn);
            var result = await conn.QueryAsync<DonationsWalletTransaction>(@"
                SELECT t.*,
                       a.Name AS AnimalName,
                       u.FullName AS DonorName
                FROM DonationsWalletTransaction t
                LEFT JOIN DonationAnimal a ON a.Id = t.IdAnimal
                LEFT JOIN Donation       d ON d.Id = t.IdDonation
               LEFT JOIN Users          u ON u.UserId = d.IdUser
                ORDER BY t.CreatedAt DESC",
                new { });
            return result.ToList();
        }
    }
}
