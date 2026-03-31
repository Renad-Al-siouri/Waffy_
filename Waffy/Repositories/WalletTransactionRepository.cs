namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class WalletTransactionRepository
    {
        private readonly IConfiguration _config;

        public WalletTransactionRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        // ✅ Get All
        public async Task<IEnumerable<WalletTransaction>> GetAllAsync()
        {
            using var db = CreateConnection();

            string sql = @"SELECT *
                       FROM WalletTransaction
                       ORDER BY Id DESC";

            return await db.QueryAsync<WalletTransaction>(sql);
        }

        // ✅ Get By Id
        public async Task<WalletTransaction?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();

            string sql = @"SELECT *
                       FROM WalletTransaction
                       WHERE Id = @Id";

            return await db.QueryFirstOrDefaultAsync<WalletTransaction>(sql, new { Id = id });
        }

        // ✅ Get By WalletId
        public async Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(int walletId)
        {
            using var db = CreateConnection();

            string sql = @"SELECT *
                       FROM WalletTransaction
                       WHERE WalletId = @WalletId
                       ORDER BY Id DESC";

            return await db.QueryAsync<WalletTransaction>(sql, new { WalletId = walletId });
        }

        // ✅ Create
        public async Task<int> CreateAsync(WalletTransaction transaction)
        {
            using var db = CreateConnection();

            string sql = @"
            INSERT INTO WalletTransaction
            (
                WalletId,
                TransactionCode,
                Type,
                Amount,
                PaymentMethod,
                Description,
                ReferenceType,
                ReferenceId,
                Status,
                CreatedAt
            )
            VALUES
            (
                @WalletId,
                @TransactionCode,
                @Type,
                @Amount,
                @PaymentMethod,
                @Description,
                @ReferenceType,
                @ReferenceId,
                @Status,
                GETDATE()
            )";

            return await db.ExecuteAsync(sql, transaction);
        }

        // ✅ Update
        public async Task<int> UpdateAsync(WalletTransaction transaction)
        {
            using var db = CreateConnection();

            string sql = @"
            UPDATE WalletTransaction
            SET
                WalletId = @WalletId,
                TransactionCode = @TransactionCode,
                Type = @Type,
                Amount = @Amount,
                PaymentMethod = @PaymentMethod,
                Description = @Description,
                ReferenceType = @ReferenceType,
                ReferenceId = @ReferenceId,
                Status = @Status
            WHERE Id = @Id";

            return await db.ExecuteAsync(sql, transaction);
        }

        // ✅ Delete
        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();

            string sql = @"DELETE FROM WalletTransaction WHERE Id = @Id";

            return await db.ExecuteAsync(sql, new { Id = id });
        }
    }
}
