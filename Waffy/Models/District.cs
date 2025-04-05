namespace Waffy.Models
{
    public class District
    {
        public int Id { get; set; }
        public int GovernorateId { get; set; }
        public string NameEn { get; set; } = string.Empty;
    }
}