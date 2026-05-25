namespace Waffy.Models
{
    public class AdoptionPost
    {
        public int IdPost { get; set; }
        public string? PostedByType { get; set; }   // "User" | "Store" | "Clinic"
        public int PostedById { get; set; }
        public int? IdPet { get; set; }              // ✅ int (NOT NULL)

        public string? StoreName { get; set; }   // ← new: cached store name
        public DateTime? BirthDate { get; set; }
        public string? PetName { get; set; }
        public string? AnimalType { get; set; }
        public string? Breed { get; set; }
        public string? Color { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? EnergyLevel { get; set; }
        public bool GoodWithChildren { get; set; }
        public bool GoodWithPets { get; set; }
        public string? HealthStatus { get; set; }
        public bool IsVaccinated { get; set; }
        public bool IsNeutered { get; set; }

        public string? City { get; set; }
        public string? Area { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }

        public string? Description { get; set; }
        public string? ReasonForAdoption { get; set; }

        public string? Status { get; set; }         // "Pending" | "Approved" | "Rejected"
        public string? AdoptionStatus { get; set; } // "Available" | "Reserved" | "Adopted"
        public string? PostCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Likes { get; set; }

    
    }
}