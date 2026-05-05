namespace Waffy.Models  // ← يجب يكون هذا بالضبط
{

    public class CartItem
    {
        public int Id { get; set; }
        public int IdCart { get; set; }
        public int IdProduct { get; set; }
        public int IdStore { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }

        public string ProductName { get; set; } = "";
        public string? ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public string StoreName { get; set; } = "";

        // ✅ هذه هي اللي ناقصة
        public List<string> Images { get; set; } = new List<string>();

        public decimal EffectivePrice =>
            SalePrice.HasValue && SalePrice > 0 ? SalePrice.Value : UnitPrice;

        public decimal LineTotal => EffectivePrice * Quantity;
    }
}