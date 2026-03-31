namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class DoctorRepository
    {
        private readonly IConfiguration _config;

        public DoctorRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Doctor>("SELECT * FROM Doctor ORDER BY Id DESC");
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Doctor>("SELECT * FROM Doctor WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Doctor>> GetByClinicIdAsync(int clinicId)
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Doctor>("SELECT * FROM Doctor WHERE ClinicId = @ClinicId ORDER BY Id DESC", new { ClinicId = clinicId });
        }

        public async Task<int> CreateAsync(Doctor item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Doctor
        (UserId, ClinicId, Specialization, SecondarySpec, ExperienceYears, Qualification,
         Certifications, Bio, Languages, WorkingDays, StartTime, EndTime,
         AppointmentDuration, MaxDailyAppointments, IsAvailable, VerificationStatus, Rating, CreatedAt)
        VALUES
        (@UserId, @ClinicId, @Specialization, @SecondarySpec, @ExperienceYears, @Qualification,
         @Certifications, @Bio, @Languages, @WorkingDays, @StartTime, @EndTime,
         @AppointmentDuration, @MaxDailyAppointments, @IsAvailable, @VerificationStatus, @Rating, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Doctor item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Doctor SET
            UserId = @UserId,
            ClinicId = @ClinicId,
            Specialization = @Specialization,
            SecondarySpec = @SecondarySpec,
            ExperienceYears = @ExperienceYears,
            Qualification = @Qualification,
            Certifications = @Certifications,
            Bio = @Bio,
            Languages = @Languages,
            WorkingDays = @WorkingDays,
            StartTime = @StartTime,
            EndTime = @EndTime,
            AppointmentDuration = @AppointmentDuration,
            MaxDailyAppointments = @MaxDailyAppointments,
            IsAvailable = @IsAvailable,
            VerificationStatus = @VerificationStatus,
            Rating = @Rating
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Doctor WHERE Id = @Id", new { Id = id });
        }
    }
}
