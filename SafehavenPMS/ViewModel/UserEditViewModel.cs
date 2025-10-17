using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class UserEditViewModel
    {
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Phone]
        [StringLength(50)]
        public string? Number { get; set; }

        // Read-only fields for display
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
