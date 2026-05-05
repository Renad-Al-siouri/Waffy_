
public class OrderItem
{
    public int Id { get; set; }
    public int IdOrder { get; set; }
    public int IdProduct { get; set; }
    public string ProductNameSnapshot { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? SalePrice { get; set; }

    public decimal EffectivePrice =>
        SalePrice.HasValue && SalePrice > 0 ? SalePrice.Value : UnitPrice;
    public decimal LineTotal => EffectivePrice * Quantity;
}
