using System;

namespace SafehavenPMS.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string UserName { get; set; } = "-";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "Info"; // Info, Success, Warning, Danger
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LinkUrl { get; set; }
    }
}