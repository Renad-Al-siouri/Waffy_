namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ClinicServiceRepository
    {
        private readonly IConfiguration _config;

        public ClinicServiceRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<ClinicService>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ClinicService>("SELECT * FROM ClinicService ORDER BY Id DESC");
        }

        public async Task<ClinicService?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<ClinicService>("SELECT * FROM ClinicService WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<ClinicService>> GetByClinicIdAsync(int clinicId)
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ClinicService>("SELECT * FROM ClinicService WHERE ClinicId = @ClinicId ORDER BY Id DESC", new { ClinicId = clinicId });
        }

        public async Task<int> CreateAsync(ClinicService item)
        {
            using var db = CreateConnection();
            string sql = "INSERT INTO ClinicService (ClinicId, ServiceName, IsActive) VALUES (@ClinicId, @ServiceName, @IsActive)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(ClinicService item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE ClinicService SET
            ClinicId = @ClinicId,
            ServiceName = @ServiceName,
            IsActive = @IsActive
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM ClinicService WHERE Id = @Id", new { Id = id });
        }
    }
}
