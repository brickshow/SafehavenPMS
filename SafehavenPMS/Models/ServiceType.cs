using System;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class ServiceType
    {
        [Key]
        public int ServiceTypeId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        //Audit Fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}