using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repository
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly string _connectionString;

        public ReservationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<int> AddAsync(AdoptionReservation reservation)
        {
            const string sql = @"
                INSERT INTO AdoptionReservation (IdPost, IdRequester, Status, Message, CreatedAt)
                VALUES (@IdPost, @IdRequester, @Status, @Message, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, reservation);
        }

        // ✅ جلب طلبات الحجز لإعلان معين — مع اسم الشخص ورقمه
        public async Task<IEnumerable<AdoptionReservation>> GetByPostIdAsync(int postId)
        {
            const string sql = @"
                SELECT r.*, u.FullName AS RequesterName, u.Phone AS RequesterPhone,
                       ap.PetName, ap.PostCode
                FROM AdoptionReservation r
                INNER JOIN Users u ON r.IdRequester = u.UserId
                INNER JOIN AdoptionPost ap ON r.IdPost = ap.IdPost
                WHERE r.IdPost = @PostId
                ORDER BY r.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionReservation>(sql, new { PostId = postId });
        }

        // ✅ جلب طلبات المستخدم الحالي
        public async Task<IEnumerable<AdoptionReservation>> GetByRequesterIdAsync(int userId)
        {
            const string sql = @"
                SELECT r.*, ap.PetName, ap.PostCode
                FROM AdoptionReservation r
                INNER JOIN AdoptionPost ap ON r.IdPost = ap.IdPost
                WHERE r.IdRequester = @UserId
                ORDER BY r.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionReservation>(sql, new { UserId = userId });
        }

        public async Task<AdoptionReservation?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT r.*, u.FullName AS RequesterName, u.Phone AS RequesterPhone,
                       ap.PetName, ap.PostCode, ap.IdPet
                FROM AdoptionReservation r
                INNER JOIN Users u ON r.IdRequester = u.UserId
                INNER JOIN AdoptionPost ap ON r.IdPost = ap.IdPost
                WHERE r.IdReservation = @Id";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<AdoptionReservation>(sql, new { Id = id });
        }

        // ✅ هل هذا المستخدم لديه طلب Pending لهذا الإعلان؟
        public async Task<bool> HasPendingAsync(int postId, int userId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM AdoptionReservation
                WHERE IdPost = @PostId AND IdRequester = @UserId AND Status = 'Pending'";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new { PostId = postId, UserId = userId }) > 0;
        }

        public async Task ApproveAsync(int reservationId)
        {
            const string sql = @"
                UPDATE AdoptionReservation
                SET Status = 'Approved'
                WHERE IdReservation = @Id";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = reservationId });
        }

        public async Task RejectAsync(int reservationId)
        {
            const string sql = @"
                UPDATE AdoptionReservation
                SET Status = 'Rejected'
                WHERE IdReservation = @Id";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = reservationId });
        }

        public async Task CancelAsync(int reservationId)
        {
            const string sql = @"
        UPDATE AdoptionReservation
        SET Status = 'Cancelled'
        WHERE IdReservation = @IdReservation";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdReservation = reservationId });

            // ✅ لو لا يوجد pending أخرى — أعد الإعلان Available
            const string checkSql = @"
        SELECT IdPost FROM AdoptionReservation
        WHERE IdReservation = @IdReservation";

            var postId = await conn.ExecuteScalarAsync<int>(checkSql, new { IdReservation = reservationId });

            const string stillPendingSql = @"
        SELECT COUNT(1) FROM AdoptionReservation
        WHERE IdPost = @PostId AND Status = 'Pending'";

            var stillPending = await conn.ExecuteScalarAsync<int>(stillPendingSql, new { PostId = postId });

            if (stillPending == 0)
            {
                const string updateSql = @"
            UPDATE AdoptionPost SET AdoptionStatus = 'Available'
            WHERE IdPost = @PostId";
                await conn.ExecuteAsync(updateSql, new { PostId = postId });
            }
        }

        public async Task<IEnumerable<AdoptionReservationAdminDto>> GetAllWithDetailsAsync()
        {
            const string sql = @"
        SELECT 
            ar.IdReservation,
            ar.IdPost,
            ap.PetName,
            ap.AnimalType,
            ap.PostedByType,
            ISNULL(ur.FullName, 'Unknown') AS RequesterName,
            ISNULL(uo.FullName, 'Unknown') AS OwnerName,
            ar.Status,
            ar.Message,
            ar.CreatedAt
        FROM AdoptionReservation ar
        INNER JOIN AdoptionPost ap ON ar.IdPost  = ap.IdPost
        LEFT JOIN  Users ur        ON ar.IdRequester = ur.UserId
        LEFT JOIN  Users uo        ON ap.PostedById  = uo.UserId
        ORDER BY ar.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionReservationAdminDto>(sql);
        }


    }
}