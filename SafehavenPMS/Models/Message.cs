using System;

namespace SafehavenPMS.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string ToUser { get; set; }
        public string FromUser { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsArchived { get; set; }
        public bool IsRead { get; set; } = false;

    }
}