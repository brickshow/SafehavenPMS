using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username or Email")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class OtpViewModel
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "OTP must be numeric.")]
        public string Otp { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}