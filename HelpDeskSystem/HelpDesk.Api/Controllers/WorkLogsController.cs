using HelpDesk.Data;
using HelpDesk.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkLogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<HelpDesk.Api.Hubs.TicketHub> _hubContext;

        public WorkLogsController(AppDbContext context, IHubContext<HelpDesk.Api.Hubs.TicketHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<WorkLog>>> GetWorkLogs(int ticketId)
        {
            return await _context.WorkLogs
                .Include(w => w.Engineer)
                .Where(w => w.TicketId == ticketId)
                .OrderByDescending(w => w.StartTime)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<WorkLog>> PostWorkLog(WorkLog workLog)
        {
            _context.WorkLogs.Add(workLog);
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.All.SendAsync("TicketUpdated", workLog.TicketId);

            return CreatedAtAction("GetWorkLogs", new { ticketId = workLog.TicketId }, workLog);
        }
    }
}
