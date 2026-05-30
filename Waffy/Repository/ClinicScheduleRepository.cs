using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repositories
{
    public class ClinicScheduleRepository : IClinicScheduleRepository
    {
        private readonly string _ConnectionString;

        public ClinicScheduleRepository(IConfiguration config)
        {
           
            _ConnectionString = config.GetConnectionString("WaffyConnection")
                ?? throw new InvalidOperationException("Connection string 'WaffyConnection' not found.");
        }

        public async Task<IEnumerable<ClinicSchedule>> GetByClinicIdAsync(int clinicId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryAsync<ClinicSchedule>(
                "SELECT * FROM ClinicSchedule WHERE IdClinic = @IdClinic",
                new { IdClinic = clinicId });
        }

        public async Task DeleteByClinicIdAsync(int clinicId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(
                "DELETE FROM ClinicSchedule WHERE IdClinic = @IdClinic",
                new { IdClinic = clinicId });
        }

        public async Task InsertAsync(ClinicSchedule schedule)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
                INSERT INTO ClinicSchedule
                    (Id, IdClinic, DayOfWeek, OpenTime, CloseTime, IsWorkingDay)
                VALUES
                    (@Id, @IdClinic, @DayOfWeek, @OpenTime, @CloseTime, @IsWorkingDay)",
                schedule);
        }

        public async Task<int> GetNextIdAsync()
        {
            using var conn = new SqlConnection(_ConnectionString);
            var max = await conn.ExecuteScalarAsync<int?>(
                "SELECT ISNULL(MAX(Id), 0) FROM ClinicSchedule");
            return (max ?? 0) + 1;
        }
    }
}