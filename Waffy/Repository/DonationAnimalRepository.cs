using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class DonationAnimalRepository : IDonationAnimalRepository
    {
        private readonly string _connectionString;

        public DonationAnimalRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<IEnumerable<DonationAnimal>> GetAllAsync()
        {
            const string sql = @"
                SELECT da.*,
                       ISNULL((SELECT SUM(d.Amount)
                                FROM Donation d
                                WHERE d.TargetId   = da.Id
                                AND   d.TargetType = 'Animal'
                                AND   d.Status     = 'Completed'), 0) AS CollectedAmount
                FROM DonationAnimal da
                ORDER BY da.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<DonationAnimal>(sql);
        }

        public async Task<IEnumerable<DonationAnimal>> GetActiveAsync()
        {
            const string sql = @"
                SELECT da.*,
                       ISNULL((SELECT SUM(d.Amount)
                                FROM Donation d
                                WHERE d.TargetId   = da.Id
                                AND   d.TargetType = 'Animal'
                                AND   d.Status     = 'Completed'), 0) AS CollectedAmount
                FROM DonationAnimal da
                WHERE da.Status = 'Active'
                ORDER BY da.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<DonationAnimal>(sql);
        }

        public async Task<int> CreateAsync(DonationAnimal animal)
        {
            const string sql = @"
                INSERT INTO DonationAnimal
                    (Name, AnimalType, Condition, Location, Story,
                     GoalAmount, Status, CreatedAt)
                VALUES
                    (@Name, @AnimalType, @Condition, @Location, @Story,
                     @GoalAmount, @Status, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                animal.Name,
                animal.AnimalType,
                animal.Condition,
                animal.Location,
                animal.Story,
                animal.GoalAmount,
                animal.Status,
                animal.CreatedAt
            });
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            const string sql = @"
                UPDATE DonationAnimal
                SET Status = @Status
                WHERE Id = @Id";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = id, Status = status });
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM DonationAnimal WHERE Id = @Id";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = id });
        }
    }
}