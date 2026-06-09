namespace Waffy.Models
{
    public class SystemPaymentSettings
    {
        public int Id { get; set; }
        public string SuperAdminCliqNumber { get; set; } = "";
        public decimal AppointmentDepositAmount { get; set; } = 5.00m;
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByAdminId { get; set; }
    }
}