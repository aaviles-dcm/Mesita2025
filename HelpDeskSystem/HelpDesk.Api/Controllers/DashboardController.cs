using HelpDesk.Data;
using HelpDesk.Data.Dtos;
using HelpDesk.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            Console.WriteLine("Dashboard API: GetStats called");
            try 
            {
                var totalTickets = await _context.Tickets.CountAsync();
                var openTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress || t.Status == TicketStatus.OnHold || t.Status == TicketStatus.Reopened);
                
                // Assuming "Resolved This Week" means tickets resolved in the last 7 days
                var lastWeek = DateTime.UtcNow.AddDays(-7);
                var resolvedThisWeek = await _context.Tickets.CountAsync(t => (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) && t.DateResolved >= lastWeek);

                var recentTickets = await _context.Tickets
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedEngineer)
                    .OrderByDescending(t => t.DateCreated)
                    .Take(5)
                    .ToListAsync();

                Console.WriteLine($"Dashboard API: Found {totalTickets} total, {recentTickets.Count} recent");

                return new DashboardStatsDto
                {
                    TotalTickets = totalTickets,
                    OpenTickets = openTickets,
                    ResolvedThisWeek = resolvedThisWeek,
                    RecentTickets = recentTickets
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dashboard API Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
