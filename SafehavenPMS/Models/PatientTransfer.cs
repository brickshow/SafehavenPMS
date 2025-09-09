// ...new file...
using System;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class PatientTransfer
    {
        //Primary Key
        [Key]
        public int TransferId { get; set; }
        public int PatientId { get; set; }            // FK to admission/patient
        public Patient? Patient { get; set; }
        public string? FromFacility { get; set; }
        public string? ToFacility { get; set; }
        public string? ProgramType { get; set; }
        public string? Reason { get; set; }
        public string? CreatedBy { get; set; }         // user who initiated transfer
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; }            // Pending, Approved, Rejected
        public DateTime? TransferDate { get; set; }   // date of transfer if
    }
}