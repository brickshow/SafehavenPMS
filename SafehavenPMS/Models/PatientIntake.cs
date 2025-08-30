using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class PatientIntake
    {
        [Key]
        public int PatientIntakeId { get; set; }
        public int PatientId { get; set; }//Foreign Key
        public Patient? Patient { get; set; }

        [Required(ErrorMessage ="Date of referral is required")]
        public DateTime DateOfReferral { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string ReferredBy { get; set; }
        public string? Affiliation { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; }
        public string? PresentingComplaint { get; set; }
        public string IntakeStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        //Other fields to be follow
    }
}
