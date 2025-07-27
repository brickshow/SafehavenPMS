using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AddNewPatientViewModel
    {
        //Foreign Keys
        public int AddressID { get; set; }
        public int ClinicalStaff { get; set; }

        //Patient Bindable Attributes
        [Required(ErrorMessage = "First Name is required")]
        public string Firstname { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        public string Lastname { get; set; }
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Sex is required")]
        public string Sex { get; set; }

        public string? Occupation { get; set; }
        public string PatientStatus { get; set; }
        public string? Education { get; set; }
        public string? Religion { get; set; }

        [Required(ErrorMessage = "Marital Status is required")]
        public string MaritalStatus { get; set; }


        [Required(ErrorMessage = "Date of Referrl is required")]
        public DateTime DateOfReferral { get; set; }
        public string? ReferredBy { get; set; }
        public string? Affiliation { get; set; }
        public string? PhotoUrl { get; set; }

    }
}
