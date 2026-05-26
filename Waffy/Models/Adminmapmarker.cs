namespace Waffy.Models
{
    public class AdminMapMarker
    {
        public int Id { get; set; }   // PK — IDENTITY
        public string MarkerType { get; set; } = "petfriendly";
        // Values: petfriendly | playground | park | clinic | store

        public string Label { get; set; } = "";
        public string? ReferenceType { get; set; }   // Clinic / Store / null
        public int? ReferenceId { get; set; }

        // ── GPS ──────────────────────────────────────────────
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // ── Location ─────────────────────────────────────────
        public string? City { get; set; }
        public string? Area { get; set; }

        // ── Details (new) ────────────────────────────────────
        public string? Phone { get; set; }   // e.g. +962 79 123 4567
        public string? WorkingHours { get; set; }   // e.g. "Daily 9AM–10PM"
        public string? Description { get; set; }   // long description
        public string? Website { get; set; }   // optional URL
        public string? ImageUrl { get; set; }   // uploaded photo path
        public string? Notes { get; set; }   // short admin notes

        // ── Display ──────────────────────────────────────────
        public string? IconColor { get; set; }   // hex color override
        public string? LogoUrl { get; set; }   // small logo/icon

        // ── Lifecycle ─────────────────────────────────────────
        public string Status { get; set; } = "Active";  // Active | Hidden
        public int? CreatedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}