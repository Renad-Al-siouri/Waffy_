namespace Waffy.Models
{
    public class Store
    {
        public int IdStore { get; set; }
        public int OwnerUserId { get; set; }
        public string Name { get; set; }
        public string? StoreType { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? WhatsApp { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool DeliveryAvailable { get; set; }
        public decimal? DeliveryFee { get; set; }   
        public string? DeliveryAreas { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public string? WorkingDays { get; set; }
        public string? AcceptedPayments { get; set; }
        public string? VerificationStatus { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public int? VerifiedByAdminId { get; set; }
        public decimal? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CliQNumber { get; set; }




    }
}