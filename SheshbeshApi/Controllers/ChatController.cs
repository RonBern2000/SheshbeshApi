using Microsoft.AspNetCore.Mvc;

namespace SheshbeshApi.Controllers
{
    public class ChatController : Controller
    {
        public ChatController()
        {
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
