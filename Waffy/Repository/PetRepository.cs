
using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;


namespace Waffy.Repository
{
    public class PetRepository : IPetRepository
    {
        private readonly string _connectionString;

        public PetRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ── Get all pets for a user ──────────────────────────
        public async Task<IEnumerable<Pet>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM Pet
                WHERE IdUsers = @UserId
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Pet>(sql, new { UserId = userId });
        }

        // ── Get single pet ───────────────────────────────────
        public async Task<Pet?> GetByIdAsync(int petId)
        {
            const string sql = "SELECT * FROM Pet WHERE IdPet = @IdPet";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Pet>(sql, new { IdPet = petId });
        }

        // ── Add new pet → return new IdPet ──────────────────
        public async Task<int> AddAsync(Pet pet)
        {
            const string sql = @"
                INSERT INTO Pet
                    (IdUsers, Name, Species, Breed, Gender, BirthDate, Color,
                     MicrochipId, Weight, VaccinationStatus, IsNeutered,
                     MedicalNotes, Traits, EnergyLevel, GoodWithChildren,
                     GoodWithPets, IsTrained, ImageUrl, CreatedAt)
                VALUES
                    (@IdUsers, @Name, @Species, @Breed, @Gender, @BirthDate, @Color,
                     @MicrochipId, @Weight, @VaccinationStatus, @IsNeutered,
                     @MedicalNotes, @Traits, @EnergyLevel, @GoodWithChildren,
                     @GoodWithPets, @IsTrained, @ImageUrl, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, pet);
        }

        // ── Update pet ───────────────────────────────────────
        public async Task UpdateAsync(Pet pet)
        {
            const string sql = @"
                UPDATE Pet SET
                    Name              = @Name,
                    Species           = @Species,
                    Breed             = @Breed,
                    Gender            = @Gender,
                    BirthDate         = @BirthDate,
                    Color             = @Color,
                    MicrochipId       = @MicrochipId,
                    Weight            = @Weight,
                    VaccinationStatus = @VaccinationStatus,
                    IsNeutered        = @IsNeutered,
                    MedicalNotes      = @MedicalNotes,
                    Traits            = @Traits,
                    EnergyLevel       = @EnergyLevel,
                    GoodWithChildren  = @GoodWithChildren,
                    GoodWithPets      = @GoodWithPets,
                    IsTrained         = @IsTrained,
                    ImageUrl          = @ImageUrl
                WHERE IdPet = @IdPet";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, pet);
        }

        // ── Delete pet ───────────────────────────────────────
        public async Task DeleteAsync(int petId)
        {
            const string sql = "DELETE FROM Pet WHERE IdPet = @IdPet";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPet = petId });
        }

        // ── Update image only ────────────────────────────────
        public async Task UpdateImageAsync(int petId, string imageUrl)
        {
            const string sql = "UPDATE Pet SET ImageUrl = @ImageUrl WHERE IdPet = @IdPet";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPet = petId, ImageUrl = imageUrl });
        }

    }
}
