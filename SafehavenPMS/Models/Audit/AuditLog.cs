using System;

namespace SafehavenPMS.Models.Audit
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Actor { get; set; }
        public string Action { get; set; }
        public string Module { get; set; }
        public string Details { get; set; }
    }
}