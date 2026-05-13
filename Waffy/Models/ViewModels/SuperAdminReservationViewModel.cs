namespace Waffy.Models.ViewModels
{
    public class SuperAdminReservationViewModel
    {
        public IEnumerable<AdoptionReservationAdminDto> Reservations { get; set; }
            = new List<AdoptionReservationAdminDto>();
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CancelledCount { get; set; }
        public string StatusFilter { get; set; } = "All";
    }
}