namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class AdoptionPost
        {
            public int Id { get; set; }
            public string PostedByType { get; set; } = null!;
            public int PostedById { get; set; }
            public int? PetId { get; set; }
            public string? PetName { get; set; }
            public string? AnimalType { get; set; }
            public string? Breed { get; set; }
            public string? Color { get; set; }
            public int? Age { get; set; }
            public string? Gender { get; set; }
            public string? EnergyLevel { get; set; }
            public bool? GoodWithChildren { get; set; }
            public bool? GoodWithPets { get; set; }
            public string? HealthStatus { get; set; }
            public bool? IsVaccinated { get; set; }
            public bool? IsNeutered { get; set; }
            public string? City { get; set; }
            public string? Area { get; set; }
            public string? ContactName { get; set; }
            public string? ContactPhone { get; set; }
            public string? Description { get; set; }
            public string? ReasonForAdoption { get; set; }
            public string Status { get; set; } = "Pending";
            public string AdoptionStatus { get; set; } = "Available";
            public string? PostCode { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
