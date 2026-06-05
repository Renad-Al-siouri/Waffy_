using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly string _conn;
        public AppointmentRepository(IConfiguration config)
        {
            _conn = config.GetConnectionString("WaffyConnection")
                ?? throw new InvalidOperationException("WaffyConnection not found.");
        }

        // ── Book ──────────────────────────────────────────────────────
        public async Task<int> BookAsync(Appointment appt)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.ExecuteScalarAsync<int>(@"
        INSERT INTO Appointment
        (
            IdUser, IdPet, IdDoctor, IdClinic,
            AppointmentDate, StartTime, EndTime,
            ServiceName, Status,
            AppointmentType, UrgencyLevel,
            Reason, Notes,
            ConsultationFee, PaymentMethod, PaymentStatus,
            CancellationReason, CreatedAt,
            ShareMedicalRecord, DepositPayMethod,CliqReference

        )
        OUTPUT INSERTED.Id
        VALUES
        (
            @IdUser, @IdPet, @IdDoctor, @IdClinic,
            @AppointmentDate, @StartTime, @EndTime,
            @ServiceName, @Status,
            @AppointmentType, @UrgencyLevel,
            @Reason, @Notes,
            @ConsultationFee, @PaymentMethod, @PaymentStatus,
            @CancellationReason, @CreatedAt,
            @ShareMedicalRecord, @DepositPayMethod,@CliqReference
        )",
                appt);
        }

        // ── Booked slots for a doctor on a date ───────────────────────
        public async Task<IEnumerable<TimeSpan>> GetBookedSlotsAsync(int idDoctor, DateTime date)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<TimeSpan>(@"
                SELECT StartTime FROM Appointment
                WHERE IdDoctor = @IdDoctor
                  AND AppointmentDate = @Date
                  AND Status NOT IN ('Cancelled')",
                new { IdDoctor = idDoctor, Date = date.Date });
        }

        // ── Check if a specific slot is free ──────────────────────────
        public async Task<bool> IsSlotAvailableAsync(int idDoctor, DateTime date, TimeSpan startTime)
        {
            using var conn = new SqlConnection(_conn);
            var count = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Appointment
                WHERE IdDoctor = @IdDoctor
                  AND AppointmentDate = @Date
                  AND StartTime = @StartTime
                  AND Status NOT IN ('Cancelled')",
                new { IdDoctor = idDoctor, Date = date.Date, StartTime = startTime });
            return count == 0;
        }

        // ── Get by Id ─────────────────────────────────────────────────
        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryFirstOrDefaultAsync<AppointmentDto>(@"
                SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                       a.ServiceName, a.Status, a.AppointmentType, a.UrgencyLevel,a.DoctorNotes,
a.DepositAmount, a.DepositPaid, a.DepositReturned,
a.AttendanceConfirmed, a.AssignedByClinic,
a.Reason, a.Notes, a.CancellationReason, a.CreatedAt,
                       a.IdDoctor, a.IdClinic, a.IdUser, a.IdPet,
                       u.FullName  AS UserFullName,  u.Phone  AS UserPhone,
                       p.Name      AS PetName,       p.Species AS PetSpecies,
                       cl.Name     AS ClinicName,
cl.CliqNumber       AS ClinicCliqNumber,
a.DepositCliqSubmitted,
                       ud.FullName AS DoctorName,
                       d.Specialization AS DoctorSpecialization,
                       d.AppointmentDuration
                FROM Appointment a
                INNER JOIN Users  u  ON a.IdUser    = u.UserId
                LEFT  JOIN Pet    p  ON a.IdPet     = p.IdPet
                INNER JOIN Clinic cl ON a.IdClinic  = cl.IdClinic
                LEFT  JOIN Doctor d  ON a.IdDoctor  = d.IdDoctor
LEFT  JOIN Users  ud ON d.IdUser    = ud.UserId
                WHERE a.Id = @Id",
                new { Id = id });
        }

        // ── By User ───────────────────────────────────────────────────
        public async Task<IEnumerable<AppointmentDto>> GetByUserIdAsync(int userId)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<AppointmentDto>(@"
                SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                       a.DepositAmount, a.DepositPaid, a.DepositReturned, a.DepositPayMethod,
                       a.ServiceName, a.Status, a.UrgencyLevel,a.CliqReference,
                       a.ShareMedicalRecord,a.MedicalRecordNote,
                       a.Reason, a.CancellationReason, a.CreatedAt,
                       a.IdDoctor, a.IdClinic, a.IdPet,
                       cl.Name     AS ClinicName,
cl.CliqNumber       AS ClinicCliqNumber,
a.DepositCliqSubmitted,
                       ud.FullName AS DoctorName,
                       d.Specialization AS DoctorSpecialization,
                       p.Name  AS PetName, p.Species AS PetSpecies
                FROM Appointment a
                INNER JOIN Clinic cl ON a.IdClinic = cl.IdClinic
               LEFT  JOIN Doctor d  ON a.IdDoctor = d.IdDoctor
LEFT  JOIN Users  ud ON d.IdUser   = ud.UserId
                LEFT  JOIN Pet    p  ON a.IdPet    = p.IdPet
                WHERE a.IdUser = @UserId
                ORDER BY a.AppointmentDate DESC, a.StartTime DESC",
                new { UserId = userId });
        }

        // ── By Doctor ─────────────────────────────────────────────────
        public async Task<IEnumerable<AppointmentDto>> GetByDoctorIdAsync(int idDoctor, string? statusFilter = null)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<AppointmentDto>(@"
                SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                       a.ServiceName, a.Status, a.UrgencyLevel,a.DoctorNotes,
                       a.ShareMedicalRecord,a.MedicalRecordNote,
a.DepositAmount, a.DepositPaid, a.DepositReturned,
a.AttendanceConfirmed, a.AssignedByClinic,
 
                       a.Reason, a.Notes, a.CancellationReason, a.CreatedAt,
                       a.IdUser, a.IdPet,
                       u.FullName  AS UserFullName, u.Phone AS UserPhone,
                       p.Name      AS PetName,      p.Species AS PetSpecies,
                       cl.Name     AS ClinicName
                FROM Appointment a
                INNER JOIN Users  u  ON a.IdUser   = u.UserId
                LEFT  JOIN Pet    p  ON a.IdPet    = p.IdPet
                INNER JOIN Clinic cl ON a.IdClinic = cl.IdClinic
                WHERE a.IdDoctor = @IdDoctor
                  AND (@StatusFilter IS NULL OR a.Status = @StatusFilter)
                ORDER BY a.AppointmentDate DESC, a.StartTime ASC",
                new
                {
                    IdDoctor = idDoctor,
                    StatusFilter = string.IsNullOrEmpty(statusFilter) ? null : statusFilter
                });
        }

        // ── By Clinic ─────────────────────────────────────────────────
        public async Task<IEnumerable<AppointmentDto>> GetByClinicIdAsync(int idClinic, string? statusFilter = null)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<AppointmentDto>(@"
                SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                       a.ServiceName, a.Status, a.UrgencyLevel,
                       a.ShareMedicalRecord,a.MedicalRecordNote,a.CliqReference,
a.DepositAmount, a.DepositPaid, a.DepositReturned,
a.AttendanceConfirmed, a.AssignedByClinic,a.DepositPayMethod,
a.CliqReference, a.Notes,a.DepositPayMethod,
a.CliqReference,
                       a.Reason, a.CancellationReason, a.CreatedAt,
                       a.IdUser, a.IdPet, a.IdDoctor,
                       u.FullName  AS UserFullName, u.Phone AS UserPhone,
                       p.Name      AS PetName,      p.Species AS PetSpecies,
                       ud.FullName AS DoctorName,
                       d.Specialization AS DoctorSpecialization
                FROM Appointment a
                INNER JOIN Users  u  ON a.IdUser   = u.UserId
                LEFT  JOIN Pet    p  ON a.IdPet    = p.IdPet
               LEFT  JOIN Doctor d  ON a.IdDoctor = d.IdDoctor
LEFT  JOIN Users  ud ON d.IdUser   = ud.UserId
                WHERE a.IdClinic = @IdClinic
                  AND (@StatusFilter IS NULL OR a.Status = @StatusFilter)
                ORDER BY a.AppointmentDate DESC, a.StartTime ASC",
                new
                {
                    IdClinic = idClinic,
                    StatusFilter = string.IsNullOrEmpty(statusFilter) ? null : statusFilter
                });
        }

        // ── Today's appointments for a doctor ─────────────────────────
        public async Task<IEnumerable<AppointmentDto>> GetTodayByDoctorIdAsync(int idDoctor)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<AppointmentDto>(@"
                SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                       a.ServiceName, a.Status, a.Reason,
                       u.FullName AS UserFullName, u.Phone AS UserPhone,
                       p.Name AS PetName, p.Species AS PetSpecies
                FROM Appointment a
                INNER JOIN Users u ON a.IdUser = u.UserId
                LEFT  JOIN Pet   p ON a.IdPet  = p.IdPet
                WHERE a.IdDoctor = @IdDoctor
                  AND a.AppointmentDate = CAST(GETDATE() AS DATE)
                  AND a.Status != 'Cancelled'
                ORDER BY a.StartTime ASC",
                new { IdDoctor = idDoctor });
        }

        // ── Cancel ────────────────────────────────────────────────────
        public async Task CancelAsync(int id, string reason)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(@"
                UPDATE Appointment
                SET Status = 'Cancelled', CancellationReason = @Reason
                WHERE Id = @Id",
                new { Id = id, Reason = reason });
        }

        // ── Complete ──────────────────────────────────────────────────
        public async Task CompleteAsync(int id)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(
                "UPDATE Appointment SET Status = 'Completed' WHERE Id = @Id",
                new { Id = id });
        }

        // ── Count bookings for a doctor on a date ─────────────────────
        public async Task<int> CountByDoctorAndDateAsync(int idDoctor, DateTime date)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Appointment
                WHERE IdDoctor = @IdDoctor
                  AND AppointmentDate = @Date
                  AND Status NOT IN ('Cancelled')",
                new { IdDoctor = idDoctor, Date = date.Date });
        }

        
        public async Task<IEnumerable<AppointmentDto>> GetTomorrowConfirmedAsync()
        {
            const string sql = @"
        SELECT
            a.Id,
            a.IdUser,
            a.IdPet,
            a.IdDoctor,
            a.IdClinic,
            a.AppointmentDate,
            a.StartTime,
            a.EndTime,
            a.ServiceName,
            a.Status,
            a.UrgencyLevel,
            a.Reason,
            a.Notes,
            a.MedicalRecordNote,
            a.ShareMedicalRecord,
            a.CancellationReason,
            a.CreatedAt,
            u.FullName   AS UserFullName,
            u.Phone      AS UserPhone,
            ud.FullName  AS DoctorName,
            c.Name       AS ClinicName,
            p.Name       AS PetName,
            p.Species    AS PetSpecies
        FROM Appointment a
        JOIN  Users  u  ON u.UserId    = a.IdUser
        LEFT  JOIN Doctor d  ON d.IdDoctor = a.IdDoctor
        LEFT  JOIN Users  ud ON ud.UserId  = d.IdUser
        LEFT  JOIN Clinic c  ON c.IdClinic = a.IdClinic
        LEFT  JOIN Pet    p  ON p.IdPet    = a.IdPet
        WHERE CAST(a.AppointmentDate AS DATE)
              = CAST(DATEADD(day, 1, GETDATE()) AS DATE)
          AND a.Status       = 'Confirmed'
          AND a.ReminderSent = 0";

            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<AppointmentDto>(sql);
        }

        public async Task MarkReminderSentAsync(int appointmentId)
        {
            const string sql = "UPDATE Appointment SET ReminderSent = 1 WHERE Id = @id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { id = appointmentId });
        }

        // في AppointmentRepository.cs — أضيفي:
        public async Task UpdateMedicalNoteAsync(int appointmentId, string? note)
        {
            const string sql = "UPDATE Appointment SET DoctorNotes = @Note WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { id = appointmentId, note });
        }

        public async Task ApproveAndAssignAsync(int appointmentId, int doctorId)
        {
            const string sql = @"
        UPDATE Appointment
        SET Status           = 'Confirmed',
            IdDoctor         = @DoctorId,
            AssignedByClinic = 1
        WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId, DoctorId = doctorId });
        }

        public async Task ApproveByDoctorAsync(int appointmentId)
        {
            const string sql = "UPDATE Appointment SET Status = 'Confirmed' WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId });
        }

        public async Task ConfirmAttendanceAsync(int appointmentId)
        {
            const string sql = "UPDATE Appointment SET AttendanceConfirmed = 1 WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId });
        }

        public async Task MarkNoShowAsync(int appointmentId)
        {
            const string sql = "UPDATE Appointment SET Status = 'NoShow' WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId });
        }

        public async Task MarkDepositPaidAsync(int appointmentId, string payMethod)
        {
            const string sql = @"
        UPDATE Appointment
        SET DepositPaid      = 1,
            DepositPayMethod = @PayMethod
        WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId, PayMethod = payMethod });
        }

        public async Task MarkDepositReturnedAsync(int appointmentId)
        {
            const string sql = "UPDATE Appointment SET DepositReturned = 1 WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId });
        }

      
        public async Task MarkCliqSubmittedAsync(int appointmentId)
        {
            const string sql =
                "UPDATE Appointment SET DepositCliqSubmitted = 1 WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId });
        }

        public async Task<IEnumerable<TimeSpan>> GetClinicBookedSlotsAsync(int clinicId, DateTime date)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<TimeSpan>(@"
        SELECT DISTINCT StartTime
        FROM Appointment
        WHERE IdClinic        = @ClinicId
          AND AppointmentDate = @Date
          AND Status NOT IN   ('Cancelled', 'NoShow')",
                new { ClinicId = clinicId, Date = date.Date });
        }

        public async Task MarkCliqRefundPendingAsync(int appointmentId)
        {
            const string sql =
                "UPDATE Appointment SET RefundStatus = 'PendingReturn' WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId });
        }
        public async Task AssignDoctorAsync(int appointmentId, int doctorId)
        {
            const string sql = @"
        UPDATE Appointment
        SET IdDoctor         = @DoctorId,
            AssignedByClinic = 1,
            Status           = 'PendingDoctorApproval'
        WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId, DoctorId = doctorId });
        }

        public async Task SetStatusAsync(int appointmentId, string status)
        {
            const string sql = "UPDATE Appointment SET Status = @Status WHERE Id = @Id";
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(sql, new { Id = appointmentId, Status = status });
        }


    }
}