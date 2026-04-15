// Models/ViewModels/UserManagementViewModel.cs
using Waffy.Models.DTOs;

namespace Waffy.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public List<UserAdminDto> Users { get; set; } = new();

        // KPIs
        public int TotalUsers { get; set; }
        public int NewThisMonth { get; set; }
        public int ActiveUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public int PendingUsers { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 8;
        public int TotalFiltered { get; set; }
        public int FromItem => TotalFiltered == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        public int ToItem => Math.Min(CurrentPage * PageSize, TotalFiltered);

        // Active filters (for keeping state)
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public string? RoleFilter { get; set; }
        public string? SortBy { get; set; }
    }
}