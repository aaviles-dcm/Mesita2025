using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Data.Entities
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TicketStatus Status { get; set; }
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public Guid CreatedById { get; set; }
        public User? CreatedBy { get; set; }
        
        public Guid? AssignedEngineerId { get; set; }
        public User? AssignedEngineer { get; set; }
        
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateResolved { get; set; }
        public DateTime? DateClosed { get; set; }
        public string? SolutionSummary { get; set; }
        public int Priority { get; set; } = 3; // 1:High, 2:Medium, 3:Low
    }

    public enum TicketStatus { New, InProgress, OnHold, Resolved, Closed, Reopened, Cancelled }
}
