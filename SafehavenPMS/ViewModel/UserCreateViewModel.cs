using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class UserCreateViewModel
    {
        [Required, StringLength(100)]
        public string Username { get; set; }

        [EmailAddress, StringLength(200)]
        public string Email { get; set; }

        [Required, StringLength(50)]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;
    }
}