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
                       u.FullName, u.Email, u.Phone
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
                    (IdDoctor, IdUser, IdClinic, Specialization, ExperienceYears,
                     Certifications, IsAvailable, VerificationStatus, CreatedAt)
                VALUES
                    (@IdDoctor, @IdUser, @IdClinic, @Specialization, @ExperienceYears,
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
    }
}