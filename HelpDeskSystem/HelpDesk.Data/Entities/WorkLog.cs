using System;

namespace HelpDesk.Data.Entities
{
    public class WorkLog
    {
        public int WorkLogId { get; set; }
        public int TicketId { get; set; }
        public Guid EngineerId { get; set; }
        public User? Engineer { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsInternal { get; set; } // Visible only to engineers
    }
}
