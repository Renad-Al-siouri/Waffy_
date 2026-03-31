namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class DoctorServiceRepository
    {
        private readonly IConfiguration _config;

        public DoctorServiceRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<DoctorService>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<DoctorService>("SELECT * FROM DoctorService ORDER BY Id DESC");
        }

        public async Task<DoctorService?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<DoctorService>("SELECT * FROM DoctorService WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DoctorService item)
        {
            using var db = CreateConnection();
            string sql = "INSERT INTO DoctorService (DoctorId, ServiceName, Price, IsActive) VALUES (@DoctorId, @ServiceName, @Price, @IsActive)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(DoctorService item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE DoctorService SET
            DoctorId = @DoctorId,
            ServiceName = @ServiceName,
            Price = @Price,
            IsActive = @IsActive
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM DoctorService WHERE Id = @Id", new { Id = id });
        }
    }
}
