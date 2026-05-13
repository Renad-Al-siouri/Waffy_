namespace Waffy.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int? IdUser { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}