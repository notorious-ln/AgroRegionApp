namespace AgroRegionApp.Models
{
    public sealed class AuthenticatedUser
    {
        public int AccountId { get; set; }
        public int? EmployeeId { get; set; }
        public string Login { get; set; }
        public string RoleName { get; set; }
        public string DisplayName { get; set; }
    }
}
