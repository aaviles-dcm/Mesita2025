using HelpDesk.Data.Entities;

namespace HelpDesk.Data.Dtos
{
    public class DashboardStatsDto
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedThisWeek { get; set; }
        public List<Ticket> RecentTickets { get; set; }
    }
}
