namespace Waffy.Models
{
    public class Order
    {
        public int IdOrder { get; set; }
        public int IdUser { get; set; }
        public int IdStore { get; set; }
        public string OrderCode { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public string? PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string RecipientName { get; set; } = "";
        public string RecipientPhone { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public decimal DeliveryFee { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }

        // ── Delivery person (assigned when shipping) ──────────────
        public string? DeliveryPersonName { get; set; }
        public string? DeliveryPersonPhone { get; set; }

        public DateTime CreatedAt { get; set; }

        // Extended — from JOIN
        public string? StoreName { get; set; }
        public int StoreOrderNumber { get; set; }

    }
}