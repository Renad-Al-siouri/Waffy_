namespace Waffy.Models
{
    namespace Waffy.Models
    {
        public class DoctorService
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }
            public string ServiceName { get; set; } = null!;
            public decimal? Price { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
