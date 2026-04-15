// Models/ViewModels/UserDetailViewModel.cs
using Waffy.Models.DTOs;

namespace Waffy.Models.ViewModels
{
    public class UserDetailViewModel
    {
        public UserAdminDto User { get; set; } = null!;

        // Counts — يُملأ لاحقاً من الـ repos المقابلة
        public int PetsCount { get; set; }
        public int AppointmentsCount { get; set; }
        public int OrdersCount { get; set; }
        public int LostReportsCount { get; set; }
        public int AdoptionCount { get; set; }
        public int CouponsCount { get; set; }

        // للـ PRG — إرجاع نفس الحالة بعد action
        public string? ReturnSearch { get; set; }
        public string? ReturnStatus { get; set; }
        public string? ReturnRole { get; set; }
        public int ReturnPage { get; set; } = 1;
    }
}