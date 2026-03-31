namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Category
        {
            public int Id { get; set; }
            public int StoreId { get; set; }
            public string Name { get; set; } = null!;
            public string? Icon { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
