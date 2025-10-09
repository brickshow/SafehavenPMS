using System;

namespace SafehavenPMS.Models
{
    public class ActivityLog
    {
        public int ActivityLogId { get; set; }
        public int? PatientId { get; set; }
        public string UserName { get; set; } = "-";
        public string Action { get; set; } = "";
        public string? Description { get; set; }
        public string Category { get; set; } = "General"; // e.g. Clinical, Profile, Security
        public string Severity { get; set; } = "Info";    // Info, Warning, Critical
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}