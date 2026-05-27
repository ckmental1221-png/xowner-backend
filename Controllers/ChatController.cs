using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XownerWebOne.Data;
using XownerWebOne.Models;

namespace XownerWebOne.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetChatHistory(int userId)
        {
            var messages = await _context.ChatMessages
                .Where(m =>
                    m.SenderId == userId || m.ReceiverId == userId
                )
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage model)
        {
            model.SenderId = 1;
            model.SentAt = DateTime.UtcNow;

            _context.ChatMessages.Add(model);

            await _context.SaveChangesAsync();

            return Ok(model);
        }
    }
}