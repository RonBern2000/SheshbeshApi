using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheshbeshApi.DAL;
using SheshbeshApi.Filters;
using SheshbeshApi.Models;

namespace SheshbeshApi.Controllers
{
    [ServiceFilter(typeof(ActionsFilter))]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IMessageRepository _messageRepository;
        public ChatController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }
        [HttpGet("between")]
        public async Task<IActionResult> GetMessagesBetween(string senderUsername, string recipientUsername)
        {
            var messages = await _messageRepository.GetMessagesAsync(senderUsername, recipientUsername);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] Message newMessage)
        {
            await _messageRepository.CreateAsync(newMessage);
            return CreatedAtAction(nameof(GetMessagesBetween), new { senderUsername = newMessage.SenderUsername, recipientUsername = newMessage.RecipientUsername }, newMessage);
        }
    }
}
