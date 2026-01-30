using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XownerWebOne.Data;

namespace XownerWebOne.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // ================= CHAT HISTORY =================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetChatHistory(int userId)
        {
            var myId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == myId && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == myId)
                )
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
