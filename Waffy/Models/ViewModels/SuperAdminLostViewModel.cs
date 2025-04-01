namespace Waffy.Models.ViewModels
{
    public class SuperAdminLostViewModel
    {
        public IEnumerable<LostPetAdminDto> Reports { get; set; }
            = new List<LostPetAdminDto>();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int FoundCount { get; set; }
        public int ExpiredCount { get; set; }
        public int PendingPaymentCount { get; set; }
        public string StatusFilter { get; set; } = "All";
        public int NoRewardConfirmedCount { get; set; }
    }
}