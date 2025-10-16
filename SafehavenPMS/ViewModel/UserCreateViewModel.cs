using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class UserCreateViewModel
    {

        [EmailAddress, StringLength(200)]
        public string Email { get; set; }

        [Required, StringLength(50)]
        public string Role { get; set; }

        public string Fullname { get; set; }

        [Phone]
        public string Number { get; set; }

        public bool IsActive { get; set; } = true;
    }
}