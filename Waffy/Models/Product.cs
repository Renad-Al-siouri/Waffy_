namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Product
        {
            public int Id { get; set; }
            public int StoreId { get; set; }
            public int? CategoryId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public decimal? SalePrice { get; set; }
            public int StockQuantity { get; set; }
            public string? PetType { get; set; }
            public string? ImageUrl { get; set; }
            public bool IsVisible { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
