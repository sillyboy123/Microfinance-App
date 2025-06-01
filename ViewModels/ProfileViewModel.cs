using System;

namespace FinPlus.ViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime MemberSince { get; set; }
    }
}
