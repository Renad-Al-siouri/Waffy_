using Waffy.Models;

namespace Waffy.Models.ViewModels
{
    public class ClinicManagementViewModel
    {
        public List<ClinicRowDto> Clinics { get; set; } = new();
        public int TotalClinics { get; set; }
        public int ActiveClinics { get; set; }
        public int PendingClinics { get; set; }
        public int SuspendedClinics { get; set; }
        public int NewThisMonth { get; set; }
        public string? StatusFilter { get; set; }
        public string? SearchQuery { get; set; }
    }

    public class ClinicRowDto
    {
        public int IdClinic { get; set; }
        public string Name { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public string OwnerEmail { get; set; } = "";
        public string OwnerPhone { get; set; } = "";
        public string City { get; set; } = "";
        public string? LogoUrl { get; set; }
        public string VerificationStatus { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int DocumentsCount { get; set; }
        public int PendingDocsCount { get; set; }
    }

    public class ClinicDetailAdminViewModel
    {
        public Clinic Clinic { get; set; } = null!;
        public string OwnerName { get; set; } = "";
        public string OwnerEmail { get; set; } = "";
        public string OwnerPhone { get; set; } = "";
        public int OwnerId { get; set; }
        public List<ClinicDocument> Documents { get; set; } = new();

        public bool AllDocsApproved =>
            Documents.Count > 0 &&
            Documents.Where(d => d.DocumentType != "Logo")
                     .All(d => d.Status == "approved" || d.Status == "Approved");

        public bool HasRejectedDoc =>
            Documents.Any(d => d.Status == "rejected" || d.Status == "Rejected");
    }
}