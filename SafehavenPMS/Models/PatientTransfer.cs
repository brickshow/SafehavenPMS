// ...new file...
using System;

namespace SafehavenPMS.Models
{
    public class PatientTransfer
    {
        public int Id { get; set; }
        public int PatientId { get; set; }            // FK to admission/patient
        public string FromFacility { get; set; }
        public string ToFacility { get; set; }
        public string ProgramType { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }         // user who initiated transfer
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}