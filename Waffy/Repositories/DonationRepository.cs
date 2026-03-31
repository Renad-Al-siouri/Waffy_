namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class DonationRepository
    {
        private readonly IConfiguration _config;

        public DonationRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Donation>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Donation>("SELECT * FROM Donation ORDER BY Id DESC");
        }

        public async Task<Donation?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Donation>(
                "SELECT * FROM Donation WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Donation item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Donation
        (DonorId, DonationType, Amount, Quantity, Unit, Description, ProofImageUrl,
         TargetType, TargetId, ReferenceCode, Status, CreatedAt)
        VALUES
        (@DonorId, @DonationType, @Amount, @Quantity, @Unit, @Description, @ProofImageUrl,
         @TargetType, @TargetId, @ReferenceCode, @Status, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Donation item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Donation SET
            DonorId = @DonorId,
            DonationType = @DonationType,
            Amount = @Amount,
            Quantity = @Quantity,
            Unit = @Unit,
            Description = @Description,
            ProofImageUrl = @ProofImageUrl,
            TargetType = @TargetType,
            TargetId = @TargetId,
            ReferenceCode = @ReferenceCode,
            Status = @Status
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Donation WHERE Id = @Id", new { Id = id });
        }
    }
}
