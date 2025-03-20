namespace Waffy.Models.ViewModels
{
    public class TopFinderDto
    {
        public int? IdUser { get; set; }
        public string FinderName { get; set; } = string.Empty;
        public string FinderPhone { get; set; } = string.Empty;
        public string? FinderEmail { get; set; }
        public int ConfirmedCount { get; set; }
        public decimal TotalRewardEarned { get; set; }
    }
}