namespace Waffy.Models.ViewModels
{
    public class ClinicPatientDto
    {
        public int IdPet { get; set; }
        public string PetName { get; set; } = "";
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public string? Gender { get; set; }
        public string? Color { get; set; }
        public string? ImageUrl { get; set; }
        public string AgeDisplay { get; set; } = "";
        public string? MicrochipId { get; set; }
        public decimal? Weight { get; set; }
        public string? VaccinationStatus { get; set; }
        public bool IsNeutered { get; set; }
        public string? MedicalNotes { get; set; }
        public string? Traits { get; set; }
        public string? EnergyLevel { get; set; }
        public bool GoodWithChildren { get; set; }
        public bool GoodWithPets { get; set; }
        public bool IsTrained { get; set; }
        public string OwnerName { get; set; } = "";
        public string? OwnerPhone { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public int TotalVisits { get; set; }
        public DateTime FirstSeen { get; set; }
        public List<PatientVisitDto> VisitHistory { get; set; } = new();
    }

    public class PatientVisitDto
    {
        public int AppointmentId { get; set; }
        public DateTime VisitDate { get; set; }
        public string? DoctorName { get; set; }
        public string? ServiceName { get; set; }
        public string? MedicalNote { get; set; }
        public string Status { get; set; } = "";
    }

    public class ClinicPatientsViewModel
    {
        public List<ClinicPatientDto> Patients { get; set; } = new();
        public int TotalPatients { get; set; }
        public int PatientsWithExpiredVacc { get; set; }
        public int PatientsVisitedThisMonth { get; set; }
    }
}