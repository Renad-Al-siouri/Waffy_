namespace Waffy.DTOs
{
    public class CustomerStoreDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }
}