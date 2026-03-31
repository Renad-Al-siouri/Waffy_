namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;

    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var db = CreateConnection();
            string sql = @"SELECT * FROM Users ORDER BY Id DESC";
            return await db.QueryAsync<User>(sql);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            string sql = @"SELECT * FROM Users WHERE Id = @Id";
            return await db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(User user)
        {
            using var db = CreateConnection();
            string sql = @"
            INSERT INTO Users
            (
                Email, PasswordHash, Phone, IsPhoneVerified, IsEmailVerified,
                FullName, City, ProfileImage, Area, AddressDetail,
                Latitude, Longitude, WhatsAppPhone, BirthDate, Gender, Bio,
                AccountStatus, WarningCount, SuspensionReason, CreatedAt, LastActiveAt, RoleId
            )
            VALUES
            (
                @Email, @PasswordHash, @Phone, @IsPhoneVerified, @IsEmailVerified,
                @FullName, @City, @ProfileImage, @Area, @AddressDetail,
                @Latitude, @Longitude, @WhatsAppPhone, @BirthDate, @Gender, @Bio,
                @AccountStatus, @WarningCount, @SuspensionReason, @CreatedAt, @LastActiveAt, @RoleId
            )";

            return await db.ExecuteAsync(sql, user);
        }

        public async Task<int> UpdateAsync(User user)
        {
            using var db = CreateConnection();
            string sql = @"
            UPDATE Users
            SET
                Email = @Email,
                PasswordHash = @PasswordHash,
                Phone = @Phone,
                IsPhoneVerified = @IsPhoneVerified,
                IsEmailVerified = @IsEmailVerified,
                FullName = @FullName,
                City = @City,
                ProfileImage = @ProfileImage,
                Area = @Area,
                AddressDetail = @AddressDetail,
                Latitude = @Latitude,
                Longitude = @Longitude,
                WhatsAppPhone = @WhatsAppPhone,
                BirthDate = @BirthDate,
                Gender = @Gender,
                Bio = @Bio,
                AccountStatus = @AccountStatus,
                WarningCount = @WarningCount,
                SuspensionReason = @SuspensionReason,
                LastActiveAt = @LastActiveAt,
                RoleId = @RoleId
            WHERE Id = @Id";

            return await db.ExecuteAsync(sql, user);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            string sql = @"DELETE FROM Users WHERE Id = @Id";
            return await db.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            using var db = CreateConnection();
            string sql = @"SELECT Id, Name FROM Roles ORDER BY Name";
            return await db.QueryAsync<Role>(sql);
        }
    }
}
