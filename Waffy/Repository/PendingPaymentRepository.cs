using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repository
{
    public class PendingPaymentRepository : IPendingPaymentRepository
    {
        private readonly string _connectionString;

        public PendingPaymentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<int> AddAsync(PendingPayment payment)
        {
            const string sql = @"
                INSERT INTO PendingPayment
                    (IdUser, Amount, Purpose, RelatedId, PaymentMethod,
                     Status, ReceiptImageUrl, CreatedAt)
                VALUES
                    (@IdUser, @Amount, @Purpose, @RelatedId, @PaymentMethod,
                     'Pending', @ReceiptImageUrl, GETUTCDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, payment);
        }

        public async Task<IEnumerable<PendingPaymentAdminDto>> GetAllPendingAsync()
        {
            const string sql = @"
                SELECT 
                    pp.IdPayment, pp.IdUser, pp.Amount, pp.Purpose,
                    pp.RelatedId, pp.PaymentMethod, pp.Status,
                    pp.ReceiptImageUrl, pp.CreatedAt,
                    ISNULL(u.FullName, 'Unknown') AS UserName,
                    ISNULL(u.Email, '')           AS UserEmail
                FROM PendingPayment pp
                LEFT JOIN Users u ON pp.IdUser = u.UserId
                WHERE pp.Status = 'Pending'
                ORDER BY pp.CreatedAt ASC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<PendingPaymentAdminDto>(sql);
        }

        public async Task ConfirmAsync(int paymentId, int adminId)
        {
            const string sql = @"
                UPDATE PendingPayment
                SET Status = 'Confirmed', ConfirmedAt = GETUTCDATE(), ConfirmedBy = @AdminId
                WHERE IdPayment = @PaymentId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { PaymentId = paymentId, AdminId = adminId });
        }

        public async Task RejectAsync(int paymentId, int adminId, string note)
        {
            const string sql = @"
                UPDATE PendingPayment
                SET Status = 'Rejected', AdminNote = @Note, ConfirmedBy = @AdminId
                WHERE IdPayment = @PaymentId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { PaymentId = paymentId, AdminId = adminId, Note = note });
        }

        public async Task<PendingPayment?> GetByIdAsync(int paymentId)
        {
            const string sql = "SELECT * FROM PendingPayment WHERE IdPayment = @Id";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<PendingPayment>(sql, new { Id = paymentId });
        }
    }
}