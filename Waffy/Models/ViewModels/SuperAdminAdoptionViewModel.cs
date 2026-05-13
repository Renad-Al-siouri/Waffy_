namespace Waffy.Models.ViewModels
{
    public class SuperAdminAdoptionViewModel
    {
        public IEnumerable<AdoptionPostAdminDto> Posts { get; set; }
            = new List<AdoptionPostAdminDto>();
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int ClosedCount { get; set; }
        public int RejectedCount { get; set; }
        public string StatusFilter { get; set; } = "All";
        public string SourceFilter { get; set; } = "All";
    }
}