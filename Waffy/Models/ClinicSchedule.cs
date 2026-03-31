namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class ClinicSchedule
        {
            public int Id { get; set; }
            public int ClinicId { get; set; }
            public int DayOfWeek { get; set; }
            public TimeSpan OpenTime { get; set; }
            public TimeSpan CloseTime { get; set; }
            public bool IsWorkingDay { get; set; }
        }
    }
}
