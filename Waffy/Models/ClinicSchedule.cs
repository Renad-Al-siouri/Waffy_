namespace Waffy.Models
{
    public class ClinicSchedule
    {
        public int Id { get; set; }
        public int? IdClinic { get; set; }
        public int? DayOfWeek { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool? IsWorkingDay { get; set; }
    }
}