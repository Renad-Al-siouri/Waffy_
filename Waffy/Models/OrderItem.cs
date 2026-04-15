namespace Waffy.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int IdOrder { get; set; }
        public int IdProduct { get; set; }
        public string ProductNameSnapshot { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? SalePrice { get; set; }
    }
}