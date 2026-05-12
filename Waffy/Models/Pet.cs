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

                var today = DateTime.Today;
                var birth = BirthDate.Value;

                int years = today.Year - birth.Year;
                int months = today.Month - birth.Month;

                // ✅ تعديل لو الشهر لم يكتمل بعد
                if (today.Day < birth.Day)
                    months--;

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                // ✅ أكبر من سنة
                if (years > 0 && months > 0)
                    return $"{years} Year{(years > 1 ? "s" : "")} & {months} Month{(months > 1 ? "s" : "")}";

                if (years > 0)
                    return $"{years} Year{(years > 1 ? "s" : "")}";

                // ✅ أقل من سنة — يعرض الشهور
                if (months > 0)
                    return $"{months} Month{(months > 1 ? "s" : "")}";

                // ✅ أقل من شهر — يعرض الأيام
                int days = (today - birth).Days;
                return days <= 0 ? "Newborn" : $"{days} Day{(days > 1 ? "s" : "")}";
            }
        }
    }
}