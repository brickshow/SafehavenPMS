using System;

namespace SafehavenPMS.Models
{
    public class PatientDocument
    {
        public int PatientDocumentId { get; set; }
        public int PatientId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? UploadedBy { get; set; }
    }
}