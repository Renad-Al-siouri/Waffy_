namespace Waffy.Models.ViewModels
{
    public class SuperAdminFoundViewModel
    {
        public IEnumerable<FoundClaimAdminDto> Claims { get; set; }
            = new List<FoundClaimAdminDto>();
        public IEnumerable<TopFinderDto> TopFinders { get; set; }
            = new List<TopFinderDto>();
        public int TotalClaims { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public decimal TotalRewardsIssued { get; set; }
        public string StatusFilter { get; set; } = "All";
    }
}