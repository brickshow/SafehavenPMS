using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AddClinicalStaffViewModel
    {
        // Temp Attributes for Clinical Staff
        public int ClinicalStaffID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Lastname { get; set; }

        public string? MiddleName { get; set; } // Optional

        [Required(ErrorMessage = "Sex is required")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Specialty is required")]
        public string Specialty { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "RPC License status is required")]
        public string RPC_Licensed { get; set; }

        [Required(ErrorMessage = "Hire Date is required")]
        public DateTime HireDate { get; set; }
        public IFormFile ImageProfile { get; set; }
        public string? Filename { get; set; }
        public string House_Unit { get; set; }
        public string Street { get; set; }
        public string Subdivision_Village { get; set; }

        [Required(ErrorMessage = "Barangay is required")]
        public string Barangay { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; }
    }
}
