using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AddPatientStep1ViewModel
    {
        [Required(ErrorMessage = "First Name is required")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Lastname { get; set; }

        public string? MiddleName { get; set; } //Accept nullable middle name

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage= "Contact Number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string ContactNumber { get; set; }

        public string? Religion { get; set; }

        [Required(ErrorMessage = "Sex is required")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }

        public string? Occupation { get; set; } //Accept nullable occupation

        [Required(ErrorMessage = "Education is required")]
        public string Education { get; set; }
    }
}
