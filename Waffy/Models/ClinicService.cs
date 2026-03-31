namespace Waffy.Models
{
    namespace Waffy.Models
    {
        public class ClinicService
        {
            public int Id { get; set; }
            public int ClinicId { get; set; }
            public string ServiceName { get; set; } = null!;
            public bool IsActive { get; set; }
        }
    }
}
