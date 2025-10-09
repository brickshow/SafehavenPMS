using System;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models.Billing
{
    public class BankInformation
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string BankName { get; set; }

        [Required, StringLength(160)]
        public string AccountName { get; set; }

        [Required, StringLength(80)]
        public string AccountNumber { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(100)]
        public string UpdatedBy { get; set; }
    }
}