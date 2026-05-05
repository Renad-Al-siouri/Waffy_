namespace Waffy.Models.ViewModels
{
    public class StoreDocumentViewModel
    {
        public int Id { get; set; }
        public string? DocumentType { get; set; }
        public string Label { get; set; } = "";
        public string? FileUrl { get; set; }
        public string? Status { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsImage => IsImageFile(FileUrl);
        public bool IsPdf => FileUrl?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) == true;

        private static bool IsImageFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            var ext = System.IO.Path.GetExtension(url).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        }
    }
}