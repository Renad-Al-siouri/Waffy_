using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repository
{
    public class AdoptionRepository : IAdoptionRepository
    {
        private readonly string _connectionString;

        public AdoptionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ── Public: Approved + Available posts ──────────────
        public async Task<IEnumerable<AdoptionPost>> GetApprovedAsync()
        {
            const string sql = @"
        SELECT ap.*, p.BirthDate
        FROM AdoptionPost ap
        LEFT JOIN Pet p ON ap.IdPet = p.IdPet
        WHERE ap.Status = 'Approved'
        AND ap.AdoptionStatus = 'Available'
        ORDER BY ap.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionPost>(sql);
        }

        // ── User's own posts ─────────────────────────────────
        public async Task<IEnumerable<AdoptionPost>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
        SELECT ap.*, p.BirthDate
        FROM AdoptionPost ap
        LEFT JOIN Pet p ON ap.IdPet = p.IdPet
        WHERE ap.PostedById = @UserId
        ORDER BY ap.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionPost>(sql, new { UserId = userId });
        }

        // ── Single post ──────────────────────────────────────
        public async Task<AdoptionPost?> GetByIdAsync(int idPost)
        {
            const string sql = "SELECT * FROM AdoptionPost WHERE IdPost = @IdPost";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<AdoptionPost>(sql, new { IdPost = idPost });
        }

        // ── Add new post → return IdPost ─────────────────────
        public async Task<int> AddAsync(AdoptionPost post)
        {
            const string sql = @"
                 INSERT INTO AdoptionPost
                    (PostedByType, PostedById, IdPet, PetName, AnimalType, Breed,
                     Color, Age, Gender, EnergyLevel, GoodWithChildren, GoodWithPets,
                     HealthStatus, IsVaccinated, IsNeutered, City, Area,
                     ContactName, ContactPhone, Description, ReasonForAdoption,
                     Status, AdoptionStatus, PostCode, CreatedAt)
                    
                VALUES
                    (@PostedByType, @PostedById, @IdPet, @PetName, @AnimalType, @Breed,
                     @Color, @Age, @Gender, @EnergyLevel, @GoodWithChildren, @GoodWithPets,
                     @HealthStatus, @IsVaccinated, @IsNeutered, @City, @Area,
                     @ContactName, @ContactPhone, @Description, @ReasonForAdoption,
                     @Status, @AdoptionStatus, @PostCode, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, post);
        }

    

        // ── Update post ──────────────────────────────────────
        public async Task UpdateAsync(AdoptionPost post)
        {
            const string sql = @"
                UPDATE AdoptionPost SET
                    PetName          = @PetName,
                    AnimalType       = @AnimalType,
                    Breed            = @Breed,
                    Color            = @Color,
                    Age              = @Age,
                    Gender           = @Gender,
                    EnergyLevel      = @EnergyLevel,
                    GoodWithChildren = @GoodWithChildren,
                    GoodWithPets     = @GoodWithPets,
                    HealthStatus     = @HealthStatus,
                    IsVaccinated     = @IsVaccinated,
                    IsNeutered       = @IsNeutered,
                    City             = @City,
                    Area             = @Area,
                    ContactName      = @ContactName,
                    ContactPhone     = @ContactPhone,
                    Description      = @Description,
                    ReasonForAdoption = @ReasonForAdoption,
                    AdoptionStatus   = @AdoptionStatus
                WHERE IdPost = @IdPost";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, post);
        }

        // ── Update AdoptionStatus only ───────────────────────
        public async Task UpdateStatusAsync(int idPost, string adoptionStatus)
        {
            const string sql = @"
                UPDATE AdoptionPost
                SET AdoptionStatus = @Status
                WHERE IdPost = @IdPost";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPost = idPost, Status = adoptionStatus });
        }

        // ── Delete ───────────────────────────────────────────
        public async Task DeleteAsync(int idPost)
        {
            const string sql = "DELETE FROM AdoptionPost WHERE IdPost = @IdPost";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPost = idPost });
        }

        // ── Add image ────────────────────────────────────────
        public async Task AddImageAsync(int idPost, string imageUrl)
        {
            const string sql = @"
                INSERT INTO AdoptionImage (IdPost, ImageUrl)
                VALUES (@IdPost, @ImageUrl)";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPost = idPost, ImageUrl = imageUrl });
        }

        // ── Get images by post ───────────────────────────────
        public async Task<IEnumerable<AdoptionImage>> GetImagesByPostIdAsync(int idPost)
        {
            const string sql = "SELECT * FROM AdoptionImage WHERE IdPost = @IdPost";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionImage>(sql, new { IdPost = idPost });
        }

        // ── Get first image only ─────────────────────────────
        public async Task<string?> GetFirstImageAsync(int idPost)
        {
            const string sql = @"
                SELECT TOP 1 ImageUrl FROM AdoptionImage
                WHERE IdPost = @IdPost";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<string>(sql, new { IdPost = idPost });



        }

        

        public async Task UpdatePostAsync(AdoptionPost post)
        {
            const string sql = @"
        UPDATE AdoptionPost SET
            City              = @City,
            Area              = @Area,
            ContactPhone      = @ContactPhone,
            Description       = @Description,
            ReasonForAdoption = @ReasonForAdoption,
            AdoptionStatus    = @AdoptionStatus
        WHERE IdPost = @IdPost
        AND PostedById = @PostedById";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, post);
        }

        public async Task ClosePostAsync(int postId)
        {
            const string sql = @"
        UPDATE AdoptionPost SET
            Status         = 'Closed',
            AdoptionStatus = 'Closed'
        WHERE IdPost = @IdPost";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPost = postId });
        }

        public async Task<bool> HasLikedAsync(int postId, int userId)
        {
            const string sql = @"
        SELECT COUNT(1) FROM AdoptionLike
        WHERE IdPost = @PostId AND IdUser = @UserId";
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new { PostId = postId, UserId = userId }) > 0;
        }

        public async Task AddLikeAsync(int postId, int userId)
        {
            const string sql = @"
        IF NOT EXISTS (SELECT 1 FROM AdoptionLike WHERE IdPost=@PostId AND IdUser=@UserId)
        BEGIN
            INSERT INTO AdoptionLike (IdPost, IdUser) VALUES (@PostId, @UserId);
            UPDATE AdoptionPost SET Likes = Likes + 1 WHERE IdPost = @PostId;
        END";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { PostId = postId, UserId = userId });
        }

        public async Task RemoveLikeAsync(int postId, int userId)
        {
            const string sql = @"
        IF EXISTS (SELECT 1 FROM AdoptionLike WHERE IdPost=@PostId AND IdUser=@UserId)
        BEGIN
            DELETE FROM AdoptionLike WHERE IdPost=@PostId AND IdUser=@UserId;
            UPDATE AdoptionPost SET Likes = CASE WHEN Likes > 0 THEN Likes-1 ELSE 0 END WHERE IdPost=@PostId;
        END";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { PostId = postId, UserId = userId });
        }

        public async Task<int> GetLikesAsync(int postId)
        {
            const string sql = "SELECT Likes FROM AdoptionPost WHERE IdPost = @Id";
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new { Id = postId });
        }

        public async Task<IEnumerable<string>> GetAllImagesAsync(int postId)
        {
            const string sql = @"
        SELECT ImageUrl
        FROM   AdoptionImage
        WHERE  IdPost = @IdPost
          AND  ImageUrl IS NOT NULL
        ORDER BY Id ASC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<string>(sql, new { IdPost = postId });
        }

        public async Task<IEnumerable<AdoptionPostAdminDto>> GetAllAsync()
        {
            const string sql = @"
        SELECT 
            ap.IdPost,
            ap.PetName,
            ap.AnimalType,
            ap.PostedByType,
            ISNULL(u.FullName, 'Unknown') AS OwnerName,
            ap.City,
            ap.Status,
            ap.AdoptionStatus,
            ap.Likes,
            ap.CreatedAt,
            ISNULL((SELECT COUNT(*) FROM PostFlag pf 
                    WHERE pf.IdPost = ap.IdPost), 0) AS FlagCount
        FROM AdoptionPost ap
        LEFT JOIN Users u ON ap.PostedById = u.UserId
        ORDER BY ap.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionPostAdminDto>(sql);
        }

        public async Task ApproveAsync(int id)
        {
            const string sql = "UPDATE AdoptionPost SET Status = 'Approved' WHERE IdPost = @Id";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = id });
        }

        public async Task RejectAsync(int id)
        {
            const string sql = "UPDATE AdoptionPost SET Status = 'Rejected' WHERE IdPost = @Id";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = id });
        }
    }
}


