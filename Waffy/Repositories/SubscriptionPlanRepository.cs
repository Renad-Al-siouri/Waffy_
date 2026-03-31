namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class SubscriptionPlanRepository
    {
        private readonly IConfiguration _config;

        public SubscriptionPlanRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<SubscriptionPlan>("SELECT * FROM SubscriptionPlan ORDER BY Id DESC");
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<SubscriptionPlan>("SELECT * FROM SubscriptionPlan WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(SubscriptionPlan item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO SubscriptionPlan
        (Name, Price, DurationInDays, MaxProducts, MaxDoctors, MaxAppointments, CommissionRate, CreatedAt)
        VALUES
        (@Name, @Price, @DurationInDays, @MaxProducts, @MaxDoctors, @MaxAppointments, @CommissionRate, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(SubscriptionPlan item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE SubscriptionPlan SET
            Name = @Name,
            Price = @Price,
            DurationInDays = @DurationInDays,
            MaxProducts = @MaxProducts,
            MaxDoctors = @MaxDoctors,
            MaxAppointments = @MaxAppointments,
            CommissionRate = @CommissionRate
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM SubscriptionPlan WHERE Id = @Id", new { Id = id });
        }
    }
}
