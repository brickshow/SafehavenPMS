using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; }


        [EmailAddress, StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;

        // store secure password representation (hash + optional salt)
        [StringLength(512)]
        public string PasswordHash { get; set; }

        [StringLength(512)]
        public string? PasswordSalt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        // Optional links to domain entities
        // use nullable FKs so a user can be either linked or standalone
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? ClinicalStaffID { get; set; }
        public ClinicalStaff? ClinicalStaff { get; set; }

        // Additional navigation (comment/uncomment if needed)
        // public ICollection<MiscellaneousItem> MiscellaneousItems { get; set; } = new List<MiscellaneousItem>();
        // public ICollection<MedicationOrder> MedicationOrders { get; set; } = new List<MedicationOrder>();
    }
}
