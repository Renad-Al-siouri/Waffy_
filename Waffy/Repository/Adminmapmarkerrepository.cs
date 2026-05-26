using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class AdminMapMarkerRepository : IAdminMapMarkerRepository
    {
        private readonly string _connectionString;

        public AdminMapMarkerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        private SqlConnection NewConn() => new(_connectionString);

        // ── Read ─────────────────────────────────────────────────────
        public async Task<IEnumerable<AdminMapMarker>> GetAllAsync()
        {
            using var conn = NewConn();
            return await conn.QueryAsync<AdminMapMarker>(
                "SELECT * FROM AdminMapMarker ORDER BY CreatedAt DESC");
        }

        public async Task<IEnumerable<AdminMapMarker>> GetActiveAsync()
        {
            const string sql = @"
                SELECT * FROM AdminMapMarker
                WHERE  Status = 'Active'
                ORDER BY MarkerType, Label";
            using var conn = NewConn();
            return await conn.QueryAsync<AdminMapMarker>(sql);
        }

        public async Task<IEnumerable<AdminMapMarker>> GetByTypeAsync(string markerType)
        {
            const string sql = @"
                SELECT * FROM AdminMapMarker
                WHERE  MarkerType = @markerType AND Status = 'Active'
                ORDER BY Label";
            using var conn = NewConn();
            return await conn.QueryAsync<AdminMapMarker>(sql, new { markerType });
        }

        public async Task<AdminMapMarker?> GetByIdAsync(int id)
        {
            using var conn = NewConn();
            return await conn.QueryFirstOrDefaultAsync<AdminMapMarker>(
                "SELECT * FROM AdminMapMarker WHERE Id = @id", new { id });
        }

        // ── Write ─────────────────────────────────────────────────────
        public async Task<int> CreateAsync(AdminMapMarker m)
        {
            const string sql = @"
                INSERT INTO AdminMapMarker
                    (MarkerType, Label, ReferenceType, ReferenceId,
                     Latitude, Longitude, City, Area,
                     Phone, WorkingHours, Description, Website,
                     ImageUrl, Notes, IconColor, LogoUrl,
                     Status, CreatedByAdminId, CreatedAt)
                VALUES
                    (@MarkerType, @Label, @ReferenceType, @ReferenceId,
                     @Latitude, @Longitude, @City, @Area,
                     @Phone, @WorkingHours, @Description, @Website,
                     @ImageUrl, @Notes, @IconColor, @LogoUrl,
                     @Status, @CreatedByAdminId, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = NewConn();
            return await conn.ExecuteScalarAsync<int>(sql, m);
        }

        public async Task UpdateAsync(AdminMapMarker m)
        {
            const string sql = @"
                UPDATE AdminMapMarker SET
                    MarkerType       = @MarkerType,
                    Label            = @Label,
                    ReferenceType    = @ReferenceType,
                    ReferenceId      = @ReferenceId,
                    Latitude         = @Latitude,
                    Longitude        = @Longitude,
                    City             = @City,
                    Area             = @Area,
                    Phone            = @Phone,
                    WorkingHours     = @WorkingHours,
                    Description      = @Description,
                    Website          = @Website,
                    ImageUrl         = @ImageUrl,
                    Notes            = @Notes,
                    IconColor        = @IconColor,
                    LogoUrl          = @LogoUrl,
                    Status           = @Status
                WHERE Id = @Id";
            using var conn = NewConn();
            await conn.ExecuteAsync(sql, m);
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            using var conn = NewConn();
            await conn.ExecuteAsync(
                "UPDATE AdminMapMarker SET Status = @status WHERE Id = @id",
                new { id, status });
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = NewConn();
            await conn.ExecuteAsync(
                "DELETE FROM AdminMapMarker WHERE Id = @id", new { id });
        }
    }
}