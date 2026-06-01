//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using XownerWebOne.Data;
//using XownerWebOne.Models;

//namespace XownerWebOne.Controllers
//{


//    [ApiController]
//    [Route("api/chat")]

//    public class ChatController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public ChatController(AppDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("{userId}")]
//        public async Task<IActionResult> GetChatHistory(int userId)
//        {
//            var messages = await _context.ChatMessages
//                .Where(m =>
//                    m.SenderId == userId || m.ReceiverId == userId
//                )
//                .OrderBy(m => m.SentAt)
//                .ToListAsync();

//            return Ok(messages);
//        }

//        [HttpPost]
//        public async Task<IActionResult> SendMessage([FromBody] ChatMessage model)
//        {
//            model.SenderId = 1;
//            model.SentAt = DateTime.UtcNow;

//            _context.ChatMessages.Add(model);

//            await _context.SaveChangesAsync();

//            return Ok(model);
//        }
//        [HttpGet("conversations")]
//        public async Task<IActionResult> GetConversations()
//        {
//            var currentUserId = 1; // abhi test ke liye

//            var userIds = await _context.ChatMessages
//                .Where(x => x.SenderId == currentUserId || x.ReceiverId == currentUserId)
//                .Select(x => x.SenderId == currentUserId
//                    ? x.ReceiverId
//                    : x.SenderId)
//                .Distinct()
//                .ToListAsync();

//            var users = await _context.Users
//                .Where(x => userIds.Contains(x.Id))
//                .Select(x => new
//                {
//                    userId = x.Id,
//                    name = x.FullName
//                })
//                .ToListAsync();

//            return Ok(users);
//        }
//    }
//}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XownerWebOne.Data;
using XownerWebOne.Models;

namespace XownerWebOne.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
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
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == currentUserId &&
                     m.ReceiverId == userId)

                    ||

                    (m.SenderId == userId &&
                     m.ReceiverId == currentUserId)
                )
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage model)
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            model.SenderId = currentUserId;
            model.SentAt = DateTime.UtcNow;

            _context.ChatMessages.Add(model);

            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var userIds = await _context.ChatMessages
                .Where(x =>
                    x.SenderId == currentUserId ||
                    x.ReceiverId == currentUserId
                )
                .Select(x =>
                    x.SenderId == currentUserId
                        ? x.ReceiverId
                        : x.SenderId
                )
                .Distinct()
                .ToListAsync();

            var users = await _context.Users
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new
                {
                    userId = x.Id,
                    name = x.FullName
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}