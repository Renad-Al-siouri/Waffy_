using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly string _ConnectionString;

        public DoctorRepository(IConfiguration config)
        {
            _ConnectionString = config.GetConnectionString("WaffyConnection")
                ?? throw new InvalidOperationException("WaffyConnection not found.");
        }

        // ✅ جلب الأطباء مع بيانات المستخدم
        public async Task<IEnumerable<DoctorListItemViewModel>> GetByClinicIdAsync(int clinicId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryAsync<DoctorListItemViewModel>(@"
                SELECT d.IdDoctor, d.IdUser, d.Specialization, d.ExperienceYears,
                       d.IsAvailable, d.VerificationStatus, d.Rating,
                       u.FullName, u.Email, u.Phone,
                       u.ProfileImage AS ProfileImageUrl
                FROM Doctor d
                INNER JOIN Users u ON d.IdUser = u.UserId
                WHERE d.IdClinic = @IdClinic
                  AND d.VerificationStatus != 'Removed'",
                new { IdClinic = clinicId });
        }

        // ✅ MAX+1 لـ IdDoctor
        public async Task<int> GetNextDoctorIdAsync()
        {
            using var conn = new SqlConnection(_ConnectionString);
            var max = await conn.ExecuteScalarAsync<int?>(
                "SELECT ISNULL(MAX(IdDoctor), 0) FROM Doctor");
            return (max ?? 0) + 1;
        }

        // ✅ تحقق من تكرار الإيميل
        public async Task<bool> EmailExistsAsync(string email)
        {
            using var conn = new SqlConnection(_ConnectionString);
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Users WHERE Email = @Email",
                new { Email = email });
            return count > 0;
        }

        // ✅ إنشاء حساب User جديد للدكتور
        public async Task<int> CreateUserAsync(string fullName, string email,
                                               string phone, string password)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Users
                    (Email, PasswordHash, RoleId, Phone,
                     IsPhoneVerified, IsEmailVerified, FullName,
                     AccountStatus, WarningCount, HasUsedRenewalDiscount, CreatedAt)
                OUTPUT INSERTED.UserId
                VALUES
                    (@Email, @Password, 4, @Phone,
                     0, 0, @FullName,
                     'Active', 0, 0, GETUTCDATE())",
                new
                {
                    Email = email,
                    Password = password,
                    Phone = phone,
                    FullName = fullName
                });
        }

        // ✅ إضافة سجل Doctor
        public async Task AddDoctorAsync(Doctor doctor)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
        INSERT INTO Doctor
            (IdUser, IdClinic, Specialization, ExperienceYears,
             Certifications, IsAvailable, VerificationStatus, CreatedAt)
        VALUES
            (@IdUser, @IdClinic, @Specialization, @ExperienceYears,
             @Certifications, @IsAvailable, @VerificationStatus, @CreatedAt)",
                doctor);
        }

        // ✅ إزالة الدكتور من العيادة (Soft delete)
        public async Task RemoveFromClinicAsync(int idDoctor)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
                UPDATE Doctor
                SET VerificationStatus = 'Removed',
                    IdClinic = NULL
                WHERE IdDoctor = @IdDoctor",
                new { IdDoctor = idDoctor });
        }

        public async Task<IEnumerable<DoctorAdminRowDto>> GetAllForAdminAsync(string? statusFilter)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryAsync<DoctorAdminRowDto>(@"
        SELECT d.IdDoctor, d.IdUser, d.IdClinic,
               d.Specialization, d.ExperienceYears,
               d.VerificationStatus, d.Certifications, d.CreatedAt,
               u.FullName, u.Email, u.Phone,
               c.Name AS ClinicName
        FROM Doctor d
        INNER JOIN Users u ON d.IdUser = u.UserId
        LEFT JOIN Clinic c ON d.IdClinic = c.IdClinic
        WHERE d.VerificationStatus != 'Removed'
          AND (@StatusFilter IS NULL OR d.VerificationStatus = @StatusFilter)
        ORDER BY d.CreatedAt DESC",
                new { StatusFilter = string.IsNullOrEmpty(statusFilter) ? null : statusFilter });
        }

        public async Task UpdateVerificationStatusAsync(int idDoctor, string status)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(
                "UPDATE Doctor SET VerificationStatus = @Status WHERE IdDoctor = @IdDoctor",
                new { Status = status, IdDoctor = idDoctor });
        }


        public async Task<DoctorDetailViewModel?> GetDetailByIdAsync(int idDoctor)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryFirstOrDefaultAsync<DoctorDetailViewModel>(@"
        SELECT d.IdDoctor, d.IdClinic,
               d.Specialization, d.SecondarySpec, d.ExperienceYears,
               d.Qualification, d.Bio, d.Languages,
               d.WorkingDays, d.StartTime, d.EndTime,
               d.Rating, d.IsAvailable, d.Certifications,
               u.FullName, u.Email, u.Phone, u.ProfileImage AS ProfileImageUrl,
               c.Name AS ClinicName, c.City AS ClinicCity
        FROM Doctor d
        INNER JOIN Users u ON d.IdUser = u.UserId
        LEFT JOIN Clinic c ON d.IdClinic = c.IdClinic
        WHERE d.IdDoctor = @IdDoctor
          AND d.VerificationStatus = 'Approved'",
                new { IdDoctor = idDoctor });
        }

        // أضيفي داخل DoctorRepository class:

        public async Task<DoctorFullDto?> GetFullInfoByUserIdAsync(int userId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryFirstOrDefaultAsync<DoctorFullDto>(@"
        SELECT d.IdDoctor, d.IdClinic,
               d.Specialization, d.SecondarySpec, d.ExperienceYears,
               d.Qualification, d.Certifications, d.Bio, d.Languages,
               d.WorkingDays, d.StartTime, d.EndTime,
               d.AppointmentDuration, d.MaxDailyAppointments,
               d.IsAvailable, d.VerificationStatus, d.Rating,
               u.FullName, u.Email, u.Phone,
           u.ProfileImage AS ProfileImageUrl, 
               c.Name AS ClinicName
        FROM Doctor d
        INNER JOIN Users u ON d.IdUser = u.UserId
        LEFT JOIN Clinic c ON d.IdClinic = c.IdClinic
        WHERE d.IdUser = @UserId",
                new { UserId = userId });
        }

        public async Task<int?> GetDoctorIdByUserIdAsync(int userId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.ExecuteScalarAsync<int?>(
                "SELECT IdDoctor FROM Doctor WHERE IdUser = @UserId",
                new { UserId = userId });
        }

        public async Task UpdatePersonalInfoAsync(int userId, string fullName,
     string? phone, string? bio, string? profileImageUrl = null)
        {
            using var conn = new SqlConnection(_ConnectionString);

            if (!string.IsNullOrEmpty(profileImageUrl))
            {
                // ✅ حفظ مع الصورة
                await conn.ExecuteAsync(@"
            UPDATE Users
            SET FullName = @FullName, Phone = @Phone, ProfileImage = @ProfileImage
            WHERE UserId = @UserId",
                    new
                    {
                        FullName = fullName,
                        Phone = phone,
                        ProfileImage = profileImageUrl,
                        UserId = userId
                    });
            }
            else
            {
                // ✅ حفظ بدون تغيير الصورة
                await conn.ExecuteAsync(@"
            UPDATE Users
            SET FullName = @FullName, Phone = @Phone
            WHERE UserId = @UserId",
                    new { FullName = fullName, Phone = phone, UserId = userId });
            }

            await conn.ExecuteAsync(
                "UPDATE Doctor SET Bio = @Bio WHERE IdUser = @UserId",
                new { Bio = bio, UserId = userId });
        }

        public async Task UpdateProfessionalInfoAsync(int userId, Doctor data)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
        UPDATE Doctor SET
            Specialization  = @Specialization,
            SecondarySpec   = @SecondarySpec,
            ExperienceYears = @ExperienceYears,
            Qualification   = @Qualification,
            Certifications  = @Certifications,
            Languages       = @Languages
        WHERE IdDoctor = @IdDoctor",   
                new
                {
                    Specialization = data.Specialization,
                    SecondarySpec = data.SecondarySpec,
                    ExperienceYears = data.ExperienceYears,
                    Qualification = data.Qualification,
                    Certifications = data.Certifications,
                    Languages = data.Languages,
                    IdDoctor = userId 
                });
        }

        public async Task UpdateWorkScheduleAsync(int userId, Doctor data)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
        UPDATE Doctor SET
            WorkingDays            = @WorkingDays,
            StartTime              = @StartTime,
            EndTime                = @EndTime,
            AppointmentDuration    = @AppointmentDuration,
            MaxDailyAppointments   = @MaxDailyAppointments,
            IsAvailable            = @IsAvailable
        WHERE IdUser = @IdUser",
                new
                {
                    data.WorkingDays,
                    data.StartTime,
                    data.EndTime,
                    data.AppointmentDuration,
                    data.MaxDailyAppointments,
                    data.IsAvailable,
                    IdUser = userId
                });
        }

        public async Task<bool> ChangePasswordAsync(int userId,
            string currentPassword, string newPassword)
        {
            using var conn = new SqlConnection(_ConnectionString);
            var rows = await conn.ExecuteAsync(@"
        UPDATE Users SET PasswordHash = @NewPassword
        WHERE UserId = @UserId AND PasswordHash = @CurrentPassword",
                new { UserId = userId, CurrentPassword = currentPassword, NewPassword = newPassword });
            return rows > 0;
        }

        public async Task<IEnumerable<DoctorDocument>> GetDocumentsByDoctorIdAsync(int idDoctor)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryAsync<DoctorDocument>(
                "SELECT * FROM DoctorDocument WHERE IdDoctor = @IdDoctor ORDER BY UploadedAt DESC",
                new { IdDoctor = idDoctor });
        }

        public async Task AddDocumentAsync(DoctorDocument doc)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
        INSERT INTO DoctorDocument
            (IdDoctor, DocumentType, FileUrl, Status, UploadedAt)
        VALUES
            (@IdDoctor, @DocumentType, @FileUrl, @Status, @UploadedAt)",
                doc);
        }

        public async Task<DoctorFullDto?> GetFullInfoByDoctorIdAsync(int idDoctor)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryFirstOrDefaultAsync<DoctorFullDto>(@"
        SELECT d.IdDoctor, d.IdClinic,
               d.Specialization, d.SecondarySpec, d.ExperienceYears,
               d.Qualification, d.Certifications, d.Bio, d.Languages,
               d.WorkingDays, d.StartTime, d.EndTime,
               d.AppointmentDuration, d.MaxDailyAppointments,
               d.IsAvailable, d.VerificationStatus, d.Rating,
               u.FullName, u.Email, u.Phone,
               u.ProfileImage AS ProfileImageUrl,
               c.Name AS ClinicName
        FROM Doctor d
        INNER JOIN Users u ON d.IdUser = u.UserId
        LEFT  JOIN Clinic c ON d.IdClinic = c.IdClinic
        WHERE d.IdDoctor = @IdDoctor",
                new { IdDoctor = idDoctor });
        }

        public async Task UpdateDocumentStatusAsync(int docId, string status)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(
                "UPDATE DoctorDocument SET Status = @Status WHERE Id = @Id",
                new { Status = status, Id = docId });
        }



    }
}