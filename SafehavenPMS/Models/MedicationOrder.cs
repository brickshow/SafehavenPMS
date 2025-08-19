using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class MedicationOrder
    {
        [Key]
        public int MedicationOrderId { get; set; }

        // Foreign Keys
        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        public int MedicineId { get; set; }
        [ForeignKey("MedicineId")]
        public Medicine Medicine { get; set; }

        // Order Details
        [Required]
        public decimal Dose { get; set; }
        public string Instruction { get; set; }
        public string Frequency { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
