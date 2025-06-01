using System.ComponentModel.DataAnnotations;

namespace MicrofinanceApp.ViewModels
{
    public class SettingsViewModel
    {
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string? CurrentPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmNewPassword { get; set; }

        [Display(Name = "Enable two-factor authentication")]
        public bool EnableTwoFactorAuth { get; set; }

        [Display(Name = "Receive email notifications")]
        public bool ReceiveEmailNotifications { get; set; } = true;

        [Display(Name = "Receive SMS notifications")]
        public bool ReceiveSmsNotifications { get; set; }

        public string? StatusMessage { get; set; }
    }
}
