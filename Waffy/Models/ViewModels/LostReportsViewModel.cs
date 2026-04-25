namespace Waffy.Models.ViewModels
{
    public class LostReportsViewModel
    {
        public List<LostPetReport> Reports { get; set; } = new();
        public User User { get; set; } = null!;
        public Dictionary<int, List<FoundPetClaim>> Finders { get; set; } = new();

        // ✅ جديد — Claims التي قدّمها هذا المستخدم بنفسه
        public List<FoundPetClaim> MyClaims { get; set; } = new();
    }
}