namespace Waffy.Models.ViewModels
{
    public class LostReportsViewModel
    {
        public List<LostPetReport> Reports { get; set; } = new();
        public User User { get; set; } = null!;
    }
}