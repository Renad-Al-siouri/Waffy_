using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly string _connectionString;

        public ComplaintRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<int> CreateAsync(Complaint complaint)
        {
            const string sql = @"
                INSERT INTO Complaint
                    (IdComplaint, IdUser, TargetType, TargetId,
                     Title, Description, Status, Priority, CreatedAt)
                SELECT ISNULL(MAX(IdComplaint), 0) + 1,
                       @IdUser, @TargetType, @TargetId,
                       @Title, @Description, @Status, @Priority, @CreatedAt
                FROM Complaint;
                SELECT ISNULL(MAX(IdComplaint), 0) FROM Complaint;";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                complaint.IdUser,
                complaint.TargetType,
                complaint.TargetId,
                complaint.Title,
                complaint.Description,
                complaint.Status,
                complaint.Priority,
                complaint.CreatedAt
            });
        }

        public async Task<IEnumerable<Complaint>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM Complaint
                WHERE IdUser = @UserId
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Complaint>(sql, new { UserId = userId });
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            const string sql = @"
                UPDATE Complaint
                SET Status = @Status
                WHERE IdComplaint = @Id";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = id, Status = status });
        }

        public async Task<IEnumerable<Complaint>> GetAllByTargetTypeAsync(string targetType)
        {
            var sql = @"
                SELECT 
                    c.IdComplaint,
                    c.IdUser,
                    c.TargetType,
                    c.TargetId,
                    c.Title,
                    c.Description,
                    c.Status,
                    c.Priority,
                    c.CreatedAt,
                    u.FullName AS ComplainantName,
                    CASE 
                        WHEN c.TargetType = 'Clinic' THEN cl.Name
                        WHEN c.TargetType = 'Store'  THEN st.Name
                        ELSE NULL
                    END AS TargetName
                FROM Complaint c
                LEFT JOIN Users u
                    ON c.IdUser = u.UserId
                LEFT JOIN Clinic cl
                    ON c.TargetId = cl.IdClinic
                    AND c.TargetType = 'Clinic'
                LEFT JOIN Store st
                    ON c.TargetId = st.IdStore
                    AND c.TargetType = 'Store'
                WHERE c.TargetType = @TargetType
                ORDER BY c.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Complaint>(sql, new { TargetType = targetType });
        }

        public async Task<IEnumerable<Complaint>> GetByStoreIdAsync(int storeId)
        {
            const string sql = @"
        SELECT c.*, u.FullName AS ComplainantName
        FROM   Complaint c
        LEFT JOIN Users u ON u.UserId = c.IdUser
        WHERE  c.TargetType = 'Store'
          AND  c.TargetId   = @StoreId
        ORDER  BY c.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Complaint>(sql, new { StoreId = storeId });
        }
    }
}