using HelpDesk.Data;
using HelpDesk.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using HelpDesk.Data.Dtos;

namespace HelpDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.DomainUsername == request.Username);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var password = await _context.UserPasswords.FirstOrDefaultAsync(p => p.UserId == user.UserId);
            if (password == null || password.Password != request.Password)
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(user);
        }
    }
}
