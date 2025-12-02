using HelpDesk.Data;
using HelpDesk.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WorkLogsController(AppDbContext context)
        {
            _context = context;
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

            return CreatedAtAction("GetWorkLogs", new { ticketId = workLog.TicketId }, workLog);
        }
    }
}
