using SafehavenPMS.Enum;

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
        public string PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }


        //One -to-Many Relationships
        public IntakeForm IntakeForm { get; set; } = new IntakeForm(); // One-to-one with IntakeForm
        public ICollection<ClinicalStaffPatient> ClinicalStaffPatients { get; set; } = new List<ClinicalStaffPatient>(); // Many-to-many with ClinicalStaff
        public ICollection<MedicationOrder> MedicationOrders { get; set; } = new List<MedicationOrder>(); // Navigation property for Medication orders
        public ICollection<AdministrationLog> AdministrationLogs { get; set; } = new List<AdministrationLog>(); // Navigation property for Medication orders
        public ICollection<NewAppointment> NewAppointments { get; set; } = new List<NewAppointment>(); // Navigation property for Schedulings
        public ICollection<InitialAssessmentForm> InitialAssessmentForms { get; set; } = new List<InitialAssessmentForm>(); // Navigation property for Initial Assessments
        public ICollection<PsychiatricAssessment> PsychiatricAssessments { get; set; } = new List<PsychiatricAssessment>(); // Navigation property for Psychiatric Assessments
        public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
        public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
        
    }
}
