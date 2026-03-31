namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ClinicScheduleRepository
    {
        private readonly IConfiguration _config;

        public ClinicScheduleRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<ClinicSchedule>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ClinicSchedule>("SELECT * FROM ClinicSchedule ORDER BY Id DESC");
        }

        public async Task<ClinicSchedule?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<ClinicSchedule>("SELECT * FROM ClinicSchedule WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<ClinicSchedule>> GetByClinicIdAsync(int clinicId)
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ClinicSchedule>("SELECT * FROM ClinicSchedule WHERE ClinicId = @ClinicId ORDER BY DayOfWeek", new { ClinicId = clinicId });
        }

        public async Task<int> CreateAsync(ClinicSchedule item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO ClinicSchedule (ClinicId, DayOfWeek, OpenTime, CloseTime, IsWorkingDay)
        VALUES (@ClinicId, @DayOfWeek, @OpenTime, @CloseTime, @IsWorkingDay)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(ClinicSchedule item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE ClinicSchedule SET
            ClinicId = @ClinicId,
            DayOfWeek = @DayOfWeek,
            OpenTime = @OpenTime,
            CloseTime = @CloseTime,
            IsWorkingDay = @IsWorkingDay
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM ClinicSchedule WHERE Id = @Id", new { Id = id });
        }
    }
}
