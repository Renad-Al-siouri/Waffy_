using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

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
                SELECT * FROM AdoptionPost
                WHERE Status = 'Approved' AND AdoptionStatus = 'Available'
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<AdoptionPost>(sql);
        }

        // ── User's own posts ─────────────────────────────────
        public async Task<IEnumerable<AdoptionPost>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM AdoptionPost
                WHERE PostedById = @UserId
                ORDER BY CreatedAt DESC";

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

    }
}


