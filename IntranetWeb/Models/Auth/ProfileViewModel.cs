namespace IntranetWeb.Models.Auth
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
