using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly string _connectionString;

        public WalletRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<Wallet?> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM Wallet
                WHERE UserId = @UserId";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Wallet>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<WalletTransaction>> GetTransactionsByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT wt.*
                FROM WalletTransaction wt
                INNER JOIN Wallet w ON wt.IdWallet = w.IdWallet
                WHERE w.UserId = @UserId
                ORDER BY wt.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<WalletTransaction>(sql, new { UserId = userId });
        }

        public async Task<int> CreateWalletAsync(int userId)
        {
            const string sql = @"
                INSERT INTO Wallet (IdWallet, UserId, Balance, Status, CreatedAt, UpdatedAt)
                SELECT ISNULL(MAX(IdWallet), 0) + 1,
                       @UserId, 0, 'Active', GETUTCDATE(), GETUTCDATE()
                FROM Wallet;
                SELECT IdWallet FROM Wallet WHERE UserId = @UserId;";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }

        public async Task AddBalanceAsync(int walletId, decimal amount)
        {
            const string sql = @"
        UPDATE Wallet
        SET Balance   = Balance + @Amount,
            UpdatedAt = GETUTCDATE()
        WHERE IdWallet = @WalletId";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Amount = amount, WalletId = walletId });
        }

        public async Task AddTransactionAsync(WalletTransaction transaction)
        {
            const string sql = @"
        INSERT INTO WalletTransaction
            (IdWalletTransaction, IdWallet, TransactionCode, Type,
             Amount, PaymentMethod, Status, CreatedAt)
        SELECT ISNULL(MAX(IdWalletTransaction), 0) + 1,
               @IdWallet, @TransactionCode, @Type,
               @Amount, @PaymentMethod, @Status, @CreatedAt
        FROM WalletTransaction";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, transaction);
        }

        public async Task DeductBalanceAsync(int walletId, decimal amount)
        {
            const string sql = @"
        UPDATE Wallet
        SET Balance   = Balance - @Amount,
            UpdatedAt = GETUTCDATE()
        WHERE IdWallet = @WalletId
          AND Balance  >= @Amount";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Amount = amount, WalletId = walletId });
        }
    }
}