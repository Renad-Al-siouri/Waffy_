namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class PetRepository
    {
        private readonly IConfiguration _config;

        public PetRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Pet>> GetAllAsync()
        {
            using var db = CreateConnection();
            string sql = "SELECT * FROM Pet ORDER BY Id DESC";
            return await db.QueryAsync<Pet>(sql);
        }

        public async Task<Pet?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            string sql = "SELECT * FROM Pet WHERE Id = @Id";
            return await db.QueryFirstOrDefaultAsync<Pet>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId)
        {
            using var db = CreateConnection();
            string sql = "SELECT * FROM Pet WHERE OwnerId = @OwnerId ORDER BY Id DESC";
            return await db.QueryAsync<Pet>(sql, new { OwnerId = ownerId });
        }

        public async Task<int> CreateAsync(Pet pet)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Pet
        (OwnerId, Name, Species, Breed, Gender, BirthDate, Color, MicrochipId, Weight,
         VaccinationStatus, IsNeutered, MedicalNotes, Traits, EnergyLevel,
         GoodWithChildren, GoodWithPets, IsTrained, ImageUrl, CreatedAt)
        VALUES
        (@OwnerId, @Name, @Species, @Breed, @Gender, @BirthDate, @Color, @MicrochipId, @Weight,
         @VaccinationStatus, @IsNeutered, @MedicalNotes, @Traits, @EnergyLevel,
         @GoodWithChildren, @GoodWithPets, @IsTrained, @ImageUrl, GETDATE())";

            return await db.ExecuteAsync(sql, pet);
        }

        public async Task<int> UpdateAsync(Pet pet)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Pet SET
            OwnerId = @OwnerId,
            Name = @Name,
            Species = @Species,
            Breed = @Breed,
            Gender = @Gender,
            BirthDate = @BirthDate,
            Color = @Color,
            MicrochipId = @MicrochipId,
            Weight = @Weight,
            VaccinationStatus = @VaccinationStatus,
            IsNeutered = @IsNeutered,
            MedicalNotes = @MedicalNotes,
            Traits = @Traits,
            EnergyLevel = @EnergyLevel,
            GoodWithChildren = @GoodWithChildren,
            GoodWithPets = @GoodWithPets,
            IsTrained = @IsTrained,
            ImageUrl = @ImageUrl
        WHERE Id = @Id";

            return await db.ExecuteAsync(sql, pet);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            string sql = "DELETE FROM Pet WHERE Id = @Id";
            return await db.ExecuteAsync(sql, new { Id = id });
        }
    }
}
