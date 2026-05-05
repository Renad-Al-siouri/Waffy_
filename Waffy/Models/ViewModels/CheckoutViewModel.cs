namespace Waffy.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public int IdStore { get; set; }
        public string StoreName { get; set; } = "";
        public decimal DeliveryFee { get; set; } = 2m;
        public List<CartItem> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.EffectivePrice * i.Quantity);
        public decimal Total => Subtotal + DeliveryFee;

        // حقول الفورم — تُملأ من Session مسبقاً
        public string RecipientName { get; set; } = "";
        public string RecipientPhone { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public string Notes { get; set; } = "";
        public string PaymentMethod { get; set; } = "Cash";
    }
}