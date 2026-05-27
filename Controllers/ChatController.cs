using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XownerWebOne.Data;
using XownerWebOne.Models;

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
        // ================= SEND MESSAGE =================
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage model)
        {
            var myId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            model.SenderId = myId;
            model.SentAt = DateTime.UtcNow;

            _context.ChatMessages.Add(model);

            await _context.SaveChangesAsync();

            return Ok(model);
        }
    }
}
