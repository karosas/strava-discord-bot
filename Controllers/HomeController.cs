using Microsoft.AspNetCore.Mvc;

namespace StravaDiscordBot.Controllers
{
    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [Route("")]
        [HttpGet]
        public IActionResult Home()
        {
            return Ok("Bot is Online");
        }
    }
}