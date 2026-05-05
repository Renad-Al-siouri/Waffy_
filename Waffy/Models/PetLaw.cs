namespace Waffy.Models
{
    
    public class PetLaw
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

       
        public string Category { get; set; } = string.Empty;

        
        public string CategoryLabel { get; set; } = string.Empty;

       
        public string Summary { get; set; } = string.Empty;

       
        public string? Source { get; set; }

        
        public DateTime LastUpdated { get; set; }

        
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}