using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class DonationRepository : IDonationRepository
    {
        private readonly string _connectionString;

        public DonationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ✅ INSERT بنمط MAX(Id)+1 — جدول Donation بدون IDENTITY
        public async Task<int> CreateAsync(Donation donation)
        {
            const string sql = @"
    INSERT INTO Donation 
        (Id, IdUser, DonationType, Amount, Quantity, Unit,
         Description, TargetType, TargetId, ReferenceCode, Status, CreatedAt)
    SELECT ISNULL(MAX(Id), 0) + 1,
           @IdUser, @DonationType, @Amount, @Quantity, @Unit,
           @Description, @TargetType, @TargetId, @ReferenceCode, @Status, @CreatedAt
    FROM Donation;
    SELECT ISNULL(MAX(Id), 0) FROM Donation;";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                donation.IdUser,
                donation.DonationType,
                donation.Amount,
                donation.Quantity,
                donation.Unit,
                donation.Description,
                donation.TargetType,
                donation.TargetId,
                donation.ReferenceCode,
                donation.Status,
                donation.CreatedAt
            });
        }

        public async Task<IEnumerable<Donation>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM Donation
                WHERE IdUser = @UserId
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Donation>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Donation>> GetAllWithDonorAsync()
        {
            const string sql = @"
        SELECT d.*,
               u.FullName AS DonorName,
               CASE 
                   WHEN d.TargetType = 'Animal' THEN da.Name
                   WHEN d.TargetType = 'General' THEN 'General Support'
                   ELSE d.TargetType
               END AS AnimalName
        FROM Donation d
        LEFT JOIN Users u          ON d.IdUser   = u.UserId
        LEFT JOIN DonationAnimal da ON d.TargetId = da.Id
                                   AND d.TargetType = 'Animal'
        ORDER BY d.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Donation>(sql);
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            const string sql = @"
        UPDATE Donation
        SET Status = @Status
        WHERE Id = @Id";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = id, Status = status });
        }


        public async Task<IEnumerable<Donation>> GetAllAsync()
        {
            const string sql = @"
        SELECT * FROM Donation
        ORDER BY CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Donation>(sql);
        }

    }

}