using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafehavenPMS.Models;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AddPatientStep1ViewModel
    {
        [Required(ErrorMessage = "First Name is required")]
        public string Firstname { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        public string Lastname { get; set; }
        public string? MiddleName { get; set; }
        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessage = "Contact Number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string ContactNumber { get; set; }
        [Required(ErrorMessage = "Sex is required")]
        public string Sex { get; set; }
        public string? Occupation { get; set; }
        [Required(ErrorMessage = "Education is required")]

        //For Education
        public int EducationLevelId { get; set; }
        public string? EducationName { get; set; }
        [BindNever]
        public IEnumerable<SelectListItem> EducationLevels { get; set; } = new List<SelectListItem>();

        //For Marital Status
        [Required(ErrorMessage = "Marital Status is required")]
        public int MaritalStatusId { get; set; }
        public string? MaritalStatusName { get; set; }
        [BindNever]
        public IEnumerable<SelectListItem> MaritalStatuses { get; set; } = new List<SelectListItem>();

        //For Religion 
        [Required(ErrorMessage = "Religion is required")]
        public int ReligionId { get; set; }
        public string? ReligionName { get; set; }
        [BindNever]
        public IEnumerable<SelectListItem> Religions { get; set; } = new List<SelectListItem>();

        //For Nationalit
        [Required(ErrorMessage = "Nationality is required")]
        public int NationalityId { get; set; }
        public string? NationalityName { get; set; }
        [BindNever]
        public IEnumerable<SelectListItem> Nationalities { get; set; } = new List<SelectListItem>();


        public string Street { get; set; }
        [Required(ErrorMessage = "Barangay is required")]
        public string Barangay { get; set; }
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; }
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }
        [Required(ErrorMessage = "Postal Code is required")]
        public string PostalCode { get; set; }
    }
}
