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
             Amount, PaymentMethod, ReferenceId, ReferenceType,
             Description, Status, BalanceAfter, CreatedAt)
        SELECT ISNULL(MAX(IdWalletTransaction), 0) + 1,
               @IdWallet, @TransactionCode, @Type,
               @Amount, @PaymentMethod, @ReferenceId, @ReferenceType,
               @Description, @Status, @BalanceAfter, @CreatedAt
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

        public async Task<Wallet?> GetOrCreateByUserIdAsync(int userId)
        {
            var wallet = await GetByUserIdAsync(userId);
            if (wallet != null) return wallet;

            await CreateWalletAsync(userId);
            return await GetByUserIdAsync(userId);
        }

        public async Task<Wallet?> GetSuperAdminWalletAsync()
        {
            using var conn = new SqlConnection(_connectionString);

            // 1) Find Super Admin user
            var adminUserId = await conn.ExecuteScalarAsync<int?>(@"
        SELECT TOP 1 u.UserId
        FROM Users u
        LEFT JOIN Roles r ON r.RoleId = u.RoleId
        WHERE r.RoleName IN ('Admin', 'SuperAdmin', 'Super Admin')
           OR u.RoleId = 5
        ORDER BY u.UserId");

            if (adminUserId == null || adminUserId == 0)
                return null;

            // 2) Check if wallet exists
            var wallet = await conn.QueryFirstOrDefaultAsync<Wallet>(@"
        SELECT *
        FROM Wallet
        WHERE UserId = @UserId",
                new { UserId = adminUserId.Value });

            if (wallet != null)
                return wallet;

            // 3) Create wallet if missing
            await conn.ExecuteAsync(@"
        INSERT INTO Wallet
            (IdWallet, UserId, Balance, Status, CreatedAt, UpdatedAt)
        SELECT ISNULL(MAX(IdWallet), 0) + 1,
               @UserId,
               0,
               'Active',
               GETUTCDATE(),
               GETUTCDATE()
        FROM Wallet",
                new { UserId = adminUserId.Value });

            // 4) Return created wallet
            return await conn.QueryFirstOrDefaultAsync<Wallet>(@"
        SELECT *
        FROM Wallet
        WHERE UserId = @UserId",
                new { UserId = adminUserId.Value });
        }

        public async Task<bool> TransferAsync(
            int fromWalletId,
            int toWalletId,
            decimal amount,
            string type,
            string paymentMethod,
            int? referenceId,
            string? referenceType,
            string? description)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                var fromBalance = await conn.ExecuteScalarAsync<decimal>(@"
            SELECT Balance
            FROM Wallet WITH (UPDLOCK, ROWLOCK)
            WHERE IdWallet = @FromWalletId",
                    new { FromWalletId = fromWalletId }, tx);

                if (fromBalance < amount)
                {
                    tx.Rollback();
                    return false;
                }

                await conn.ExecuteAsync(@"
            UPDATE Wallet
            SET Balance = Balance - @Amount,
                UpdatedAt = SYSUTCDATETIME()
            WHERE IdWallet = @FromWalletId",
                    new { Amount = amount, FromWalletId = fromWalletId }, tx);

                await conn.ExecuteAsync(@"
            UPDATE Wallet
            SET Balance = Balance + @Amount,
                UpdatedAt = SYSUTCDATETIME()
            WHERE IdWallet = @ToWalletId",
                    new { Amount = amount, ToWalletId = toWalletId }, tx);

                var fromAfter = await conn.ExecuteScalarAsync<decimal>(
                    "SELECT Balance FROM Wallet WHERE IdWallet = @Id",
                    new { Id = fromWalletId }, tx);

                var toAfter = await conn.ExecuteScalarAsync<decimal>(
                    "SELECT Balance FROM Wallet WHERE IdWallet = @Id",
                    new { Id = toWalletId }, tx);

                var code = $"{type}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                await conn.ExecuteAsync(@"
            INSERT INTO WalletTransaction
                (IdWalletTransaction, IdWallet, TransactionCode, Type,
                 Amount, PaymentMethod, ReferenceId, ReferenceType,
                 Description, Status, BalanceAfter, CreatedAt)
            SELECT ISNULL(MAX(IdWalletTransaction), 0) + 1,
                   @IdWallet, @TransactionCode, @Type,
                   @Amount, @PaymentMethod, @ReferenceId, @ReferenceType,
                   @Description, 'Completed', @BalanceAfter, SYSUTCDATETIME()
            FROM WalletTransaction",
                    new
                    {
                        IdWallet = fromWalletId,
                        TransactionCode = code,
                        Type = type,
                        Amount = -amount,
                        PaymentMethod = paymentMethod,
                        ReferenceId = referenceId,
                        ReferenceType = referenceType,
                        Description = description,
                        BalanceAfter = fromAfter
                    }, tx);

                await conn.ExecuteAsync(@"
            INSERT INTO WalletTransaction
                (IdWalletTransaction, IdWallet, TransactionCode, Type,
                 Amount, PaymentMethod, ReferenceId, ReferenceType,
                 Description, Status, BalanceAfter, CreatedAt)
            SELECT ISNULL(MAX(IdWalletTransaction), 0) + 1,
                   @IdWallet, @TransactionCode, @Type,
                   @Amount, @PaymentMethod, @ReferenceId, @ReferenceType,
                   @Description, 'Completed', @BalanceAfter, SYSUTCDATETIME()
            FROM WalletTransaction",
                    new
                    {
                        IdWallet = toWalletId,
                        TransactionCode = code,
                        Type = type,
                        Amount = amount,
                        PaymentMethod = paymentMethod,
                        ReferenceId = referenceId,
                        ReferenceType = referenceType,
                        Description = description,
                        BalanceAfter = toAfter
                    }, tx);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> RefundFromSuperAdminToUserAsync(
            int userId,
            decimal amount,
            int appointmentId,
            int? adminId,
            string description)
        {
            var userWallet = await GetOrCreateByUserIdAsync(userId);
            var adminWallet = await GetSuperAdminWalletAsync();

            if (userWallet == null || adminWallet == null)
                return false;

            return await TransferAsync(
                adminWallet.IdWallet,
                userWallet.IdWallet,
                amount,
                "AppointmentDepositRefund",
                "Waffy Wallet",
                appointmentId,
                "Appointment",
                description);
        }

        public async Task<SystemPaymentSettings?> GetPaymentSettingsAsync()
        {
            const string sql = @"
        SELECT TOP 1 *
        FROM SystemPaymentSettings
        ORDER BY Id DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<SystemPaymentSettings>(sql);
        }

        public async Task UpdateSuperAdminCliqNumberAsync(string cliqNumber, int adminId)
        {
            const string sql = @"
        IF EXISTS (SELECT 1 FROM SystemPaymentSettings)
        BEGIN
            UPDATE SystemPaymentSettings
            SET SuperAdminCliqNumber = @CliqNumber,
                UpdatedAt = SYSUTCDATETIME(),
                UpdatedByAdminId = @AdminId
            WHERE Id = (SELECT TOP 1 Id FROM SystemPaymentSettings ORDER BY Id DESC)
        END
        ELSE
        BEGIN
            INSERT INTO SystemPaymentSettings
                (SuperAdminCliqNumber, AppointmentDepositAmount, UpdatedAt, UpdatedByAdminId)
            VALUES
                (@CliqNumber, 5.00, SYSUTCDATETIME(), @AdminId)
        END";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { CliqNumber = cliqNumber, AdminId = adminId });
        }

        public async Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(int walletId)
        {
            const string sql = @"
        SELECT *
        FROM WalletTransaction
        WHERE IdWallet = @WalletId
        ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<WalletTransaction>(sql, new { WalletId = walletId });
        }
    }
}