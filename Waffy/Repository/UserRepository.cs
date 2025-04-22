using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.DTOs;
using Waffy.Models.ViewModels;

namespace Waffy.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly IRoleRepository _roleRepository;

        public UserRepository(IConfiguration configuration, IRoleRepository roleRepository)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("Connection string not found");
            _roleRepository = roleRepository;
        }

        // ── Get All ──────────────────────────────────────
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>("SELECT * FROM Users");
        }

        // ── Get By ID ─────────────────────────────────────
        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserId = @UserId",
                new { UserId = userId });
        }

        // ── Get By Email ──────────────────────────────────
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = email });
        }

        // ── Insert (shared) ───────────────────────────────
        private async Task InsertUserAsync(User user)
        {
            var sql = @"
                INSERT INTO Users 
                (
                    Email, PasswordHash, RoleId, Phone, IsPhoneVerified,
                    IsEmailVerified, FullName, City, ProfileImage, Area,
                    AddressDetail, Latitude, Longitude, WhatsAppPhone,
                    BirthDate, Gender, Bio, AccountStatus, WarningCount,
                    SuspensionReason, CreatedAt, LastActiveAt
                )
                VALUES 
                (
                    @Email, @PasswordHash, @RoleId, @Phone, @IsPhoneVerified,
                    @IsEmailVerified, @FullName, @City, @ProfileImage, @Area,
                    @AddressDetail, @Latitude, @Longitude, @WhatsAppPhone,
                    @BirthDate, @Gender, @Bio, @AccountStatus, @WarningCount,
                    @SuspensionReason, @CreatedAt, @LastActiveAt
                )";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, user);
        }

        // ── Add Normal User ───────────────────────────────
        public async Task AddNormalUserAsync(User user)
        {
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("User");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;
            await InsertUserAsync(user);
        }

        // ── Add Clinic User ───────────────────────────────
        public async Task AddClinicAsync(User user)
        {
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("Clinic");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;
            await InsertUserAsync(user);
        }

        // ── Add Store User ────────────────────────────────
        public async Task AddStoreAsync(User user)
        {
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("Store");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;
            await InsertUserAsync(user);
        }

        // ── Add Doctor User ───────────────────────────────
        public async Task AddDoctorAsync(User user)
        {
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("Doctor");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;
            await InsertUserAsync(user);
        }

        // ── Update ────────────────────────────────────────
        public async Task UpdateAsync(User user)
        {
            var sql = @"
                UPDATE Users SET
                    Email            = @Email,
                    PasswordHash     = @PasswordHash,
                    Phone            = @Phone,
                    IsPhoneVerified  = @IsPhoneVerified,
                    IsEmailVerified  = @IsEmailVerified,
                    FullName         = @FullName,
                    City             = @City,
                    ProfileImage     = @ProfileImage,
                    Area             = @Area,
                    AddressDetail    = @AddressDetail,
                    Latitude         = @Latitude,
                    Longitude        = @Longitude,
                    WhatsAppPhone    = @WhatsAppPhone,
                    BirthDate        = @BirthDate,
                    Gender           = @Gender,
                    Bio              = @Bio,
                    AccountStatus    = @AccountStatus,
                    WarningCount     = @WarningCount,
                    SuspensionReason = @SuspensionReason,
                    LastActiveAt     = @LastActiveAt
                WHERE UserId = @UserId";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, user);
        }

        // ── Delete ────────────────────────────────────────
        public async Task DeleteAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "DELETE FROM Users WHERE UserId = @UserId",
                new { UserId = userId });
        }

        // ── Login ─────────────────────────────────────────
        public async Task<LoginUserResult?> LoginAsync(string email, string passwordHash)
        {
            var sql = @"
                SELECT 
                    u.UserId,
                    u.Email,
                    u.FullName,
                    u.RoleId,
                    r.RoleName,
                    u.AccountStatus
                FROM Users u
                INNER JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.Email        = @Email
                  AND u.PasswordHash = @PasswordHash";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<LoginUserResult>(sql, new
            {
                Email = email,
                PasswordHash = passwordHash
            });
        }

        // ════════════════════════════════════════════════
        // Admin Methods
        // ════════════════════════════════════════════════

        // ── Get All For Admin (with filters + pagination) ─
        public async Task<IEnumerable<UserAdminDto>> GetAllForAdminAsync(
            string? search, string? status, string? roleName,
            string sortBy, int page, int pageSize)
        {
            var orderBy = sortBy switch
            {
                "oldest" => "u.CreatedAt ASC",
                "name" => "u.FullName ASC",
                "status" => "u.AccountStatus ASC",
                _ => "u.CreatedAt DESC"
            };

            var sql = $@"
                SELECT u.UserId, u.FullName, u.Email, u.Phone,
                       u.City, u.Area, u.Gender, u.Bio,
                       u.ProfileImage, u.AccountStatus, u.CreatedAt,
                       r.RoleName
                FROM   Users u
                JOIN   Roles r ON u.RoleId = r.RoleId
                WHERE  r.RoleName != 'Admin'
                  AND (@Search   IS NULL OR u.FullName LIKE '%'+@Search+'%'
                                         OR u.Email    LIKE '%'+@Search+'%'
                                         OR u.Phone    LIKE '%'+@Search+'%')
                  AND (@Status   IS NULL OR u.AccountStatus = @Status)
                  AND (@RoleName IS NULL OR r.RoleName      = @RoleName)
                ORDER BY {orderBy}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<UserAdminDto>(sql, new
            {
                Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                Status = string.IsNullOrWhiteSpace(status) ? null : status,
                RoleName = string.IsNullOrWhiteSpace(roleName) ? null : roleName,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });
        }

        // ── Get Single User For Admin ─────────────────────
        public async Task<UserAdminDto?> GetByIdForAdminAsync(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<UserAdminDto>(@"
                SELECT u.UserId, u.FullName, u.Email, u.Phone,
                       u.City, u.Area, u.Gender, u.Bio,
                       u.ProfileImage, u.AccountStatus, u.CreatedAt,
                       r.RoleName
                FROM   Users u
                JOIN   Roles r ON u.RoleId = r.RoleId
                WHERE  u.UserId = @UserId",
                new { UserId = userId });
        }

        // ── Count Filtered ────────────────────────────────
        public async Task<int> CountFilteredAsync(
            string? search, string? status, string? roleName)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Users u
                JOIN Roles r ON u.RoleId = r.RoleId
                WHERE r.RoleName != 'Admin'
                  AND (@Search   IS NULL OR u.FullName LIKE '%'+@Search+'%'
                                         OR u.Email    LIKE '%'+@Search+'%'
                                         OR u.Phone    LIKE '%'+@Search+'%')
                  AND (@Status   IS NULL OR u.AccountStatus = @Status)
                  AND (@RoleName IS NULL OR r.RoleName      = @RoleName)",
                new
                {
                    Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                    Status = string.IsNullOrWhiteSpace(status) ? null : status,
                    RoleName = string.IsNullOrWhiteSpace(roleName) ? null : roleName
                });
        }

        // ── Count By Status ───────────────────────────────
        public async Task<int> CountByStatusAsync(string status)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Users u
                JOIN Roles r ON u.RoleId = r.RoleId
                WHERE r.RoleName != 'Admin'
                  AND u.AccountStatus = @Status",
                new { Status = status });
        }

        // ── Count New This Month ──────────────────────────
        public async Task<int> CountNewThisMonthAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Users u
                JOIN Roles r ON u.RoleId = r.RoleId
                WHERE r.RoleName != 'Admin'
                  AND MONTH(u.CreatedAt) = MONTH(GETDATE())
                  AND YEAR(u.CreatedAt)  = YEAR(GETDATE())");
        }

        // ── Count Total ───────────────────────────────────
        public async Task<int> CountTotalAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Users u
                JOIN Roles r ON u.RoleId = r.RoleId
                WHERE r.RoleName != 'Admin'");
        }

        // ── Update Status ─────────────────────────────────
        public async Task UpdateStatusAsync(int userId, string newStatus)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "UPDATE Users SET AccountStatus = @Status WHERE UserId = @UserId",
                new { Status = newStatus, UserId = userId });
        }

        public async Task<string> GetRoleNameByIdAsync(int roleId)
        {
            var sql = "SELECT RoleName FROM Roles WHERE RoleId = @RoleId";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<string>(sql, new { RoleId = roleId }) ?? "User";
        }
        public async Task<IEnumerable<int>> GetUserIdsByCityAsync(string city)
        {
            const string sql = @"
        SELECT UserId FROM Users
        WHERE City = @City AND AccountStatus = 'Active'";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<int>(sql, new { City = city });
        }
        public async Task<IEnumerable<UserCityDto>> GetUsersByCityAsync(
    string city, string? area = null)
        {
            const string sql = @"
        SELECT
            UserId,
            FullName,
            Phone,
            City,
            Area,
            CASE
                WHEN @Area IS NOT NULL AND Area = @Area THEN 1
                ELSE 2
            END AS ProximityRank
        FROM   Users
        WHERE  City          = @City
          AND  AccountStatus = 'Active'
        ORDER BY ProximityRank, FullName";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<UserCityDto>(
                sql, new { City = city, Area = area });
        }

        public async Task UpdateRenewalDiscountAsync(int userId)
        {
            const string sql = @"
        UPDATE Users SET HasUsedRenewalDiscount = 1
        WHERE UserId = @UserId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { UserId = userId });
        }

    } 
}