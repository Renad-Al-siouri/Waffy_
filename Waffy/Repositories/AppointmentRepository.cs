namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class AppointmentRepository
    {
        private readonly IConfiguration _config;

        public AppointmentRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Appointment>("SELECT * FROM Appointment ORDER BY Id DESC");
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Appointment>("SELECT * FROM Appointment WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Appointment item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Appointment
        (UserId, PetId, DoctorId, ClinicId, AppointmentDate, StartTime, EndTime, ServiceName,
         Status, AppointmentType, UrgencyLevel, Reason, Notes, ConsultationFee, PaymentMethod,
         PaymentStatus, BookingDeposit, DepositStatus, CancellationReason, CreatedAt)
        VALUES
        (@UserId, @PetId, @DoctorId, @ClinicId, @AppointmentDate, @StartTime, @EndTime, @ServiceName,
         @Status, @AppointmentType, @UrgencyLevel, @Reason, @Notes, @ConsultationFee, @PaymentMethod,
         @PaymentStatus, @BookingDeposit, @DepositStatus, @CancellationReason, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Appointment item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Appointment SET
            UserId = @UserId,
            PetId = @PetId,
            DoctorId = @DoctorId,
            ClinicId = @ClinicId,
            AppointmentDate = @AppointmentDate,
            StartTime = @StartTime,
            EndTime = @EndTime,
            ServiceName = @ServiceName,
            Status = @Status,
            AppointmentType = @AppointmentType,
            UrgencyLevel = @UrgencyLevel,
            Reason = @Reason,
            Notes = @Notes,
            ConsultationFee = @ConsultationFee,
            PaymentMethod = @PaymentMethod,
            PaymentStatus = @PaymentStatus,
            BookingDeposit = @BookingDeposit,
            DepositStatus = @DepositStatus,
            CancellationReason = @CancellationReason
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Appointment WHERE Id = @Id", new { Id = id });
        }
    }
}
