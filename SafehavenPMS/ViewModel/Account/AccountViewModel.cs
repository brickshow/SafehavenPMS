using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AccountViewModel
    {
        [Required, StringLength(100)]
        public string Username { get; set; }

        // plain password used for input (not persisted as plain text)
        [Required, DataType(DataType.Password), StringLength(100)]
        public string? Password { get; set; }

        // optional recovery email
        [EmailAddress, StringLength(200)]
        public string RecoveryEmail { get; set; }

        public string? Firstname { get; set; }

        public string? Lastname { get; set; }
    
        public string?   MiddleName { get; set; }
        public string? Email { get; set; }

        [Phone, StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }
        public string? Position { get; set; }
        public bool IsActive { get; set; } = true;

        // store secure password representation (hash + optional salt)
        [StringLength(512)]
        public string? PasswordHash { get; set; }

        [StringLength(512)]
        public string? PasswordSalt { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        // Optional links to domain entities
        // use nullable FKs so a user can be either linked or standalone
        public int? PatientId { get; set; }

        public int? ClinicalStaffId { get; set; }

        // Additional navigation (comment/uncomment if needed)
        // public ICollection<MiscellaneousItem> MiscellaneousItems { get; set; } = new List<MiscellaneousItem>();
        // public ICollection<MedicationOrder> MedicationOrders { get; set; } = new List<MedicationOrder>();
    }
}