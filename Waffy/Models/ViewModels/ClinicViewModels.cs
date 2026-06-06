using Waffy.Models;

namespace Waffy.Models.ViewModels
{
    public class ClinicCardViewModel
    {
        public int IdClinic { get; set; }
        public string Name { get; set; } = "";
        public string? ClinicType { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Phone { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool HasEmergency { get; set; }
        public TimeSpan? DefaultOpenTime { get; set; }
        public TimeSpan? DefaultCloseTime { get; set; }
        public List<string> Services { get; set; } = new();
        public string? Description { get; set; }
        public decimal Rating { get; set; }
    }

    public class DoctorDetailViewModel
    {
        public int IdDoctor { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public string? SecondarySpec { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Qualification { get; set; }
        public string? Bio { get; set; }
        public string? Languages { get; set; }
        public string? WorkingDays { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public decimal? Rating { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Certifications { get; set; }
        // Clinic info
        public int? IdClinic { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicCity { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<DoctorDocument> CertificationDocs { get; set; } = new();
        public List<Waffy.Models.Review> Reviews { get; set; } = new();
        public bool CurrentUserHasReviewed { get; set; }
        public int ReviewCount { get; set; }
    }

    public class ClinicsListViewModel
    {
        public List<ClinicCardViewModel> Clinics { get; set; } = new();
        public List<string> Cities { get; set; } = new();
        public List<string> AllServices { get; set; } = new();
        public string SelectedCity { get; set; } = "All";
        public string? SearchQuery { get; set; }
        public string SelectedService { get; set; } = "All";
    }

    public class ClinicDetailViewModel
    {
        public int IdClinic { get; set; }
        public string Name { get; set; } = "";
        public string? ClinicType { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool HasEmergency { get; set; }
        public TimeSpan? DefaultOpenTime { get; set; }
        public TimeSpan? DefaultCloseTime { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal Rating { get; set; }
        public List<string> Services { get; set; } = new();
        public List<ClinicSchedule> Schedule { get; set; } = new();
        public List<ClinicImage> GalleryImages { get; set; } = new();
        public List<DoctorListItemViewModel> Doctors { get; set; } = new();
        public List<Waffy.Models.Review> Reviews { get; set; } = new();
        public bool CurrentUserHasReviewed { get; set; }
        public int ReviewCount { get; set; }
    }
}