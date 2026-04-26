using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using XownerWebOne.Data;
using XownerWebOne.Models;

namespace XownerWebOne.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // USER CONNECT
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        // USER DISCONNECT
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // SEND MESSAGE
        public async Task SendMessage(int receiverId, string message)
        {
            var senderId = int.Parse(
                Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var chat = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message
            };

            _context.ChatMessages.Add(chat);
            await _context.SaveChangesAsync();

            //// receiver
            //await Clients.Group(receiverId.ToString())
            //    .SendAsync("ReceiveMessage", new
            //    {
            //        senderId,
            //        message
            //    });

            //// sender (own message)
            //await Clients.Group(senderId.ToString())
            //    .SendAsync("ReceiveMessage", new
            //    {
            //        senderId,
            //        message
            //    });
            await Clients.Group(receiverId.ToString())
    .SendAsync("ReceiveMessage", new
    {
        senderId,
        receiverId,
        message,
        sentAt = DateTime.UtcNow
    });

            // sender ko bhi bhejo
            await Clients.Group(senderId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    senderId,
                    receiverId,
                    message,
                    sentAt = DateTime.UtcNow
                });
        }
    }
}
