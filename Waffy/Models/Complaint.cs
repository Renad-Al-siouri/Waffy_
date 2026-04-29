namespace Waffy.Models
{
    public class Complaint
    {
        public int IdComplaint { get; set; }
        public int? IdUser { get; set; }
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}