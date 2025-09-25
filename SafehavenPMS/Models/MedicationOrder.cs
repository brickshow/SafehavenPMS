using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class MedicationOrder
    {
        [Key]
        public int MedicationOrderId { get; set; }

        public int? AdministrationLogId { get; set; }

        // Patient
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        // Medicine
        [Required]
        public int MedicineId { get; set; }

        [ForeignKey("MedicineId")]
        public Medicine? Medicine { get; set; }

        //Problem List
        public int? PsyProblemListId { get; set; }
        [ForeignKey("PsyProblemListId")]
        public PsyProblemList? PsyProblem { get; set; }

        // Dosage
        [Required]
        [Range(1, int.MaxValue)]
        public int UnitPerDose { get; set; }

        public string? Note { get; set; }

        // Schedule
        [Required]
        public string ScheduledType { get; set; } // "Daily" or "NonDaily"

        public int? DaysInterval { get; set; } // Enabled only for NonDaily

        public bool Breakfast { get; set; } = true;
        public bool Lunch { get; set; } = true;
        public bool Dinner { get; set; } = true;
        public bool Bedtime { get; set; } = false;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? DiscontinueDate { get; set; }
        public bool NoDiscontinueDate { get; set; }

        // Audit
        [Required]
        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public ICollection<AdministrationLog> AdministrationLogs { get; set; }//Navigation property for Medication
    }
}
