namespace Waffy.Models
{
    public class AdoptionReservation
    {
        public int IdReservation { get; set; }
        public int IdPost { get; set; }
        public int IdRequester { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ للعرض فقط — من JOIN
        public string? RequesterName { get; set; }
        public string? RequesterPhone { get; set; }
        public string? PetName { get; set; }
        public string? PostCode { get; set; }
    }
}