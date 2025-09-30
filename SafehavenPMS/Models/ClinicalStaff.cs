using SafehavenPMS.Controllers;

namespace SafehavenPMS.Models
{
    public class ClinicalStaff
    {
        //Personal Information
        public int ClinicalStaffID { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string? MiddleName { get; set; }//Accept null
        public string Sex { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string ProfilePictureURL { get; set; }
        public string PRC_Licensed { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<ClinicalStaffPatient> ClinicalStaffPatients { get; set; } // Many-to-many with Patient
        public ICollection<Availability> Availabilities { get; set; } //Navigation property
        public ICollection<NewAppointment> Schedulings { get; set; } // Navigation property for Schedulings
    }
}
