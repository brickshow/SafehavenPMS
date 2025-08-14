namespace SafehavenPMS.Models
{
    public class Patient
    {
        //Personal Information
        public int PatientId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Sex { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PatientStatus { get; set; }
        public string Occupation { get; set; }
        public string Education { get; set; }
        public string Religion { get; set; }
        public string MaritalStatus { get; set; }
        public DateTime DateOfReferral { get; set; }
        public string ReferredBy { get; set; }
        public string? Affiliation { get; set; }
        public string PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }


        //One -to-Many Relationships
        public ICollection<ClinicalStaffPatient> ClinicalStaffPatients { get; set; } // Many-to-many with ClinicalStaff
        public ICollection<Appointment> Appointments { get; set; } // Navigation property for appointments
    }
}
