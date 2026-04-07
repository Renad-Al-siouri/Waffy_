namespace Waffy.Models
{
    // This model is returned after a successful login.
    // RoleName is used to redirect the user to the correct dashboard.
    public class LoginUserResult
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string AccountStatus { get; set; }
    }
}