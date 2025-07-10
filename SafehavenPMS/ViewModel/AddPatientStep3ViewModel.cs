using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AddPatientStep3ViewModel
    {
        //Adding fields for case details
        [Required(ErrorMessage = "Please date of Intake")]
        public  DateTime DateOfIntake { get; set; }

        [Required(ErrorMessage ="Please add date of Referral")]
        public DateTime DateOfReferral { get; set; }

        public string Affilation { get; set; }

        public string ReferredBy { get; set; }

        [Required(ErrorMessage ="Please add Phycisian")]
        public string Physician { get; set; }
    }
}
