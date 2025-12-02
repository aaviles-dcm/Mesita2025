using System;

namespace HelpDesk.Data.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string LinkUrl { get; set; } // Link to relevant ticket
    }
}
