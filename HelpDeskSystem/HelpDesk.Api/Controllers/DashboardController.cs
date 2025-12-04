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
        public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] Guid? userId, [FromQuery] UserRole? role)
        {
            Console.WriteLine($"Dashboard API: GetStats called for User={userId}, Role={role}");
            try 
            {
                var ticketsQuery = _context.Tickets.AsQueryable();

                if (userId.HasValue && role.HasValue)
                {
                    if (role == UserRole.Engineer)
                    {
                        // Engineers see tickets assigned to them OR (Status == New AND Assigned == null)
                        ticketsQuery = ticketsQuery.Where(t => t.AssignedEngineerId == userId || (t.AssignedEngineerId == null && t.Status == TicketStatus.New));
                    }
                    else if (role == UserRole.User)
                    {
                        // Users see only their own tickets
                        ticketsQuery = ticketsQuery.Where(t => t.CreatedById == userId);
                    }
                    // Admins see everything (no filter applied)
                }

                var totalTickets = await ticketsQuery.CountAsync();
                var openTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress || t.Status == TicketStatus.OnHold || t.Status == TicketStatus.Reopened);
                
                // Assuming "Resolved This Week" means tickets resolved in the last 7 days
                var lastWeek = DateTime.UtcNow.AddDays(-7);
                var resolvedThisWeek = await ticketsQuery.CountAsync(t => (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) && t.DateResolved >= lastWeek);

                var recentTickets = await ticketsQuery
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
