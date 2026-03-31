namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Pet
        {
            public int Id { get; set; }
            public int OwnerId { get; set; }
            public string Name { get; set; } = null!;
            public string Species { get; set; } = null!;
            public string? Breed { get; set; }
            public string Gender { get; set; } = null!;
            public DateTime? BirthDate { get; set; }
            public string? Color { get; set; }
            public string? MicrochipId { get; set; }
            public decimal? Weight { get; set; }
            public string? VaccinationStatus { get; set; }
            public bool IsNeutered { get; set; }
            public string? MedicalNotes { get; set; }
            public string? Traits { get; set; }
            public string? EnergyLevel { get; set; }
            public bool? GoodWithChildren { get; set; }
            public bool? GoodWithPets { get; set; }
            public bool IsTrained { get; set; }
            public string? ImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
