namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class SubscriptionRepository
    {
        private readonly IConfiguration _config;

        public SubscriptionRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Subscription>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Subscription>("SELECT * FROM Subscription ORDER BY Id DESC");
        }

        public async Task<Subscription?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Subscription>("SELECT * FROM Subscription WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Subscription item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Subscription
        (PlanId, EntityType, EntityId, StartDate, EndDate, AmountPaid, PaymentStatus, Status, CreatedAt)
        VALUES
        (@PlanId, @EntityType, @EntityId, @StartDate, @EndDate, @AmountPaid, @PaymentStatus, @Status, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Subscription item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Subscription SET
            PlanId = @PlanId,
            EntityType = @EntityType,
            EntityId = @EntityId,
            StartDate = @StartDate,
            EndDate = @EndDate,
            AmountPaid = @AmountPaid,
            PaymentStatus = @PaymentStatus,
            Status = @Status
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Subscription WHERE Id = @Id", new { Id = id });
        }
    }
}
