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
    public class DoctorsViewModel
    {
        public List<DoctorListItemViewModel> Doctors { get; set; } = new();
    }

    public class DoctorListItemViewModel
    {
        public int IdDoctor { get; set; }
        public int? IdUser { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public bool? IsAvailable { get; set; }
        public string? VerificationStatus { get; set; }
        public decimal? Rating { get; set; }
    }

    public class AddDoctorViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        
        public IFormFile? CertificationFile { get; set; }
    }

    public class DoctorAdminRowDto
    {
        public int IdDoctor { get; set; }
        public int? IdUser { get; set; }
        public int? IdClinic { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public string? VerificationStatus { get; set; }
        public string? Certifications { get; set; }
        public string? ClinicName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class DoctorsAdminViewModel
    {
        public List<DoctorAdminRowDto> Doctors { get; set; } = new();
        public int TotalDoctors { get; set; }
        public int PendingDoctors { get; set; }
        public int ApprovedDoctors { get; set; }
        public int RejectedDoctors { get; set; }
        public int ClinicsWithPendingDocs { get; set; }
        public string? StatusFilter { get; set; }
    }
    public class ClinicBasicInfoViewModel
    {
        public string Name { get; set; } = "";
        public string? ClinicType { get; set; }
        public string City { get; set; } = "";
        public string? Area { get; set; }
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
    public class WorkingHoursViewModel
    {
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public string OpenTime { get; set; } = "09:00";
        public string CloseTime { get; set; } = "18:00";
        public bool HasEmergency { get; set; }
    }

    public class ServicesViewModel
    {
        public List<string> SelectedServices { get; set; } = new();
    }
    public class DocumentsCViewModel
    {
        public List<ClinicDocument> Documents { get; set; } = new();
        public string ClinicVerificationStatus { get; set; } = "";
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