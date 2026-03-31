namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class AdoptionPostRepository
    {
        private readonly IConfiguration _config;

        public AdoptionPostRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<AdoptionPost>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<AdoptionPost>("SELECT * FROM AdoptionPost ORDER BY Id DESC");
        }

        public async Task<AdoptionPost?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<AdoptionPost>(
                "SELECT * FROM AdoptionPost WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(AdoptionPost item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO AdoptionPost
        (PostedByType, PostedById, PetId, PetName, AnimalType, Breed, Color, Age, Gender,
         EnergyLevel, GoodWithChildren, GoodWithPets, HealthStatus, IsVaccinated, IsNeutered,
         City, Area, ContactName, ContactPhone, Description, ReasonForAdoption, Status,
         AdoptionStatus, PostCode, CreatedAt)
        VALUES
        (@PostedByType, @PostedById, @PetId, @PetName, @AnimalType, @Breed, @Color, @Age, @Gender,
         @EnergyLevel, @GoodWithChildren, @GoodWithPets, @HealthStatus, @IsVaccinated, @IsNeutered,
         @City, @Area, @ContactName, @ContactPhone, @Description, @ReasonForAdoption, @Status,
         @AdoptionStatus, @PostCode, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(AdoptionPost item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE AdoptionPost SET
            PostedByType = @PostedByType,
            PostedById = @PostedById,
            PetId = @PetId,
            PetName = @PetName,
            AnimalType = @AnimalType,
            Breed = @Breed,
            Color = @Color,
            Age = @Age,
            Gender = @Gender,
            EnergyLevel = @EnergyLevel,
            GoodWithChildren = @GoodWithChildren,
            GoodWithPets = @GoodWithPets,
            HealthStatus = @HealthStatus,
            IsVaccinated = @IsVaccinated,
            IsNeutered = @IsNeutered,
            City = @City,
            Area = @Area,
            ContactName = @ContactName,
            ContactPhone = @ContactPhone,
            Description = @Description,
            ReasonForAdoption = @ReasonForAdoption,
            Status = @Status,
            AdoptionStatus = @AdoptionStatus,
            PostCode = @PostCode
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM AdoptionPost WHERE Id = @Id", new { Id = id });
        }
    }
}
