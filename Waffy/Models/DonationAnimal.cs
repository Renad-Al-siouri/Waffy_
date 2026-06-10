namespace Waffy.Models
{
    public class DonationAnimal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Story { get; set; } = string.Empty;
        public decimal? GoalAmount { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal CollectedAmount { get; set; } 
    }
}