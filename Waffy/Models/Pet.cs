namespace Waffy.Models
{
    public class Pet
    {
        public int IdPet { get; set; }
        public int IdUsers { get; set; }
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Color { get; set; }
        public string? MicrochipId { get; set; }
        public decimal? Weight { get; set; }
        public string? VaccinationStatus { get; set; }
        public bool IsNeutered { get; set; }
        public string? MedicalNotes { get; set; }
        public string? Traits { get; set; }
        public string? EnergyLevel { get; set; }
        public bool GoodWithChildren { get; set; }
        public bool GoodWithPets { get; set; }
        public bool IsTrained { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Display helper — هذا مقبول لأنه حساب وليس UI text
        public string AgeDisplay
        {
            get
            {
                if (!BirthDate.HasValue) return "Unknown";
                int years = DateTime.Today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.DayOfYear > DateTime.Today.DayOfYear) years--;
                return years <= 0 ? "< 1 Year" : $"{years} Year{(years > 1 ? "s" : "")}";
            }
        }
    }
}