using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.Filters;
using SheshbeshApi.Hubs;
using SheshbeshApi.Services;

namespace SheshbeshApi.Controllers
{
    [ServiceFilter(typeof(ActionsFilter))]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return BadRequest("Message cannot be empty.");
            }

            await _chatService.SendMessage(msg);

            return Ok(new { Message = "Message sent successfully!" });
        }
        [HttpPost("sendDm")]
        public async Task<IActionResult> SendDmMessage([FromBody] string msg, string id)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return BadRequest("Message cannot be empty.");
            }

            await _chatService.SendDmMessage(msg , id);

            return Ok(new { Message = msg });
        }
    }
}
