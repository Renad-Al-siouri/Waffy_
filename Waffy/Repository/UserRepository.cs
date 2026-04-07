using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

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

        // -----------------------------------------------
        // Get all users
        // -----------------------------------------------
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var sql = "SELECT * FROM Users";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>(sql);
        }

        // -----------------------------------------------
        // Get user by ID
        // -----------------------------------------------
        public async Task<User?> GetByIdAsync(int userId)
        {
            var sql = "SELECT * FROM Users WHERE UserId = @UserId";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
        }

        // -----------------------------------------------
        // Get user by email
        // -----------------------------------------------
        public async Task<User?> GetByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        // -----------------------------------------------
        // Shared private insert method
        // All Add methods call this after setting RoleId
        // -----------------------------------------------
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

        // -----------------------------------------------
        // Register a normal User (Role = "User")
        // -----------------------------------------------
        public async Task AddNormalUserAsync(User user)
        {
            // Always fetch RoleId by name — never hardcode numbers
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("User");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;

            await InsertUserAsync(user);
        }

        // -----------------------------------------------
        // Register a Clinic (Role = "Clinic")
        // -----------------------------------------------
        public async Task AddClinicAsync(User user)
        {
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("Clinic");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;

            await InsertUserAsync(user);
        }

        // -----------------------------------------------
        // Register a Store (Role = "Store")
        // -----------------------------------------------
        public async Task AddStoreAsync(User user)
        {
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("Store");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;

            await InsertUserAsync(user);
        }

        // -----------------------------------------------
        // A Clinic creates a Doctor account (Role = "Doctor")
        // Doctor does NOT register themselves
        // -----------------------------------------------
        public async Task AddDoctorAsync(User user)
        {
            user.RoleId = await _roleRepository.GetRoleIdByNameAsync("Doctor");
            user.CreatedAt = DateTime.UtcNow;
            user.WarningCount = 0;

            await InsertUserAsync(user);
        }

        // -----------------------------------------------
        // Update user profile
        // -----------------------------------------------
        public async Task UpdateAsync(User user)
        {
            var sql = @"
                UPDATE Users SET
                    Email               = @Email,
                    PasswordHash        = @PasswordHash,
                    Phone               = @Phone,
                    IsPhoneVerified     = @IsPhoneVerified,
                    IsEmailVerified     = @IsEmailVerified,
                    FullName            = @FullName,
                    City                = @City,
                    ProfileImage        = @ProfileImage,
                    Area                = @Area,
                    AddressDetail       = @AddressDetail,
                    Latitude            = @Latitude,
                    Longitude           = @Longitude,
                    WhatsAppPhone       = @WhatsAppPhone,
                    BirthDate           = @BirthDate,
                    Gender              = @Gender,
                    Bio                 = @Bio,
                    AccountStatus       = @AccountStatus,
                    WarningCount        = @WarningCount,
                    SuspensionReason    = @SuspensionReason,
                    LastActiveAt        = @LastActiveAt
                WHERE UserId = @UserId";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, user);
        }

        // -----------------------------------------------
        // Delete a user by ID
        // -----------------------------------------------
        public async Task DeleteAsync(int userId)
        {
            var sql = "DELETE FROM Users WHERE UserId = @UserId";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        // -----------------------------------------------
        // Login
        // JOINs with Roles to return RoleName
        // Used to redirect user to the correct dashboard
        // -----------------------------------------------
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
    }
}