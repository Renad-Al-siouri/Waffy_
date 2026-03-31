namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class AdminMapMarkerRepository
    {
        private readonly IConfiguration _config;

        public AdminMapMarkerRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<AdminMapMarker>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<AdminMapMarker>("SELECT * FROM AdminMapMarker ORDER BY Id DESC");
        }

        public async Task<AdminMapMarker?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<AdminMapMarker>(
                "SELECT * FROM AdminMapMarker WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(AdminMapMarker item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO AdminMapMarker
        (MarkerType, ReferenceType, ReferenceId, Latitude, Longitude, Status, CreatedByAdminId, CreatedAt)
        VALUES
        (@MarkerType, @ReferenceType, @ReferenceId, @Latitude, @Longitude, @Status, @CreatedByAdminId, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(AdminMapMarker item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE AdminMapMarker SET
            MarkerType = @MarkerType,
            ReferenceType = @ReferenceType,
            ReferenceId = @ReferenceId,
            Latitude = @Latitude,
            Longitude = @Longitude,
            Status = @Status,
            CreatedByAdminId = @CreatedByAdminId
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM AdminMapMarker WHERE Id = @Id", new { Id = id });
        }
    }
}
