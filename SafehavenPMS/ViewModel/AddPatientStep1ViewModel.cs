using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AddPatientStep1ViewModel
    {
        [Required]
        public string PatientID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Lastname { get; set; }

        public string? MiddleName { get; set; } //Accept nullable middle name

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage= "Contact Number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string ContactNumber { get; set; }


        [Required(ErrorMessage = "Sex is required")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }

        public string? Occupation { get; set; } //Accept nullable occupation    

        // --- for Education ---
        [Required(ErrorMessage = "Education is required")]
        public int EducationLevelId { get; set; }
        public IEnumerable<SelectListItem> EducationLevels { get; set; }

        // --- for Marital Status ---
        [Required(ErrorMessage = "Marital Status is required")]
        public int MaritalStatusId { get; set; }
        public IEnumerable<SelectListItem> MaritalStatuses { get; set; }

        //for Religion
        public int ReligionId { get; set; }
        public IEnumerable<SelectListItem> Religions { get; set; }

        //For Nationality
        public int NationalityId { get; set; }
        public IEnumerable<SelectListItem> Nationalities { get; set; }
    }
}
