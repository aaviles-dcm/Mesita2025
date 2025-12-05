using HelpDesk.Data;
using HelpDesk.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<HelpDesk.Api.Hubs.TicketHub> _hubContext;

        public TicketsController(AppDbContext context, IHubContext<HelpDesk.Api.Hubs.TicketHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets([FromQuery] Guid? userId, [FromQuery] UserRole? role)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedEngineer)
                .AsQueryable();

            if (userId.HasValue && role.HasValue)
            {
                if (role == UserRole.Engineer)
                {
                    // Engineers see tickets assigned to them OR unassigned tickets (optional, but usually they need to pick them)
                    // For now, let's show assigned tickets AND unassigned tickets so they can pick them?
                    // The requirement says: "el ingeniero podra ver solo los tickets asignados a el"
                    // But also "el puede tomar el tiket". To take it, he must see it first.
                    // Usually "New" tickets are visible to all engineers or there is a pool.
                    // Let's assume they can see AssignedTo them OR (Status == New AND Assigned == null)
                    query = query.Where(t => t.AssignedEngineerId == userId || (t.AssignedEngineerId == null && t.Status == TicketStatus.New));
                }
                else if (role == UserRole.User)
                {
                    // Users see only their own tickets
                    query = query.Where(t => t.CreatedById == userId);
                }
            }

            return await query.ToListAsync();
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] Guid engineerId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.AssignedEngineerId = engineerId;
            ticket.Status = TicketStatus.InProgress; // Auto move to InProgress? Or keep New? Let's say InProgress.
            
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("TicketUpdated", id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedEngineer)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            // Auto-assign engineer based on Category
            var category = await _context.Categories
                .Include(c => c.Engineers)
                .FirstOrDefaultAsync(c => c.CategoryId == ticket.CategoryId);

            if (category != null && category.Engineers.Any())
            {
                // Simple strategy: Pick the first available engineer
                // Future improvement: Round-robin or load balancing
                var engineer = category.Engineers.First();
                ticket.AssignedEngineerId = engineer.UserId;
                // ticket.Status = TicketStatus.New; // Keep as New, but assigned
            }

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            // No need to notify on creation for details view, but maybe for dashboard? 
            // Let's notify anyway just in case.
            await _hubContext.Clients.All.SendAsync("TicketUpdated", ticket.TicketId);

            return CreatedAtAction("GetTicket", new { id = ticket.TicketId }, ticket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return BadRequest();
            }

            _context.Entry(ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("TicketUpdated", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("TicketUpdated", id); // Notify deletion?

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }
    }
}
