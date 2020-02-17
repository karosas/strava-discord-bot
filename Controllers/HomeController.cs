using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace StravaDiscordBot.Controllers
{
    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        [Route("")]
        [HttpGet]
        public IActionResult Home()
        {
            _logger.LogInformation("Home Reached");
            _logger.LogWarning("Home Reached");
            _logger.LogError("Home Reached");
            return Ok("Bot is Online");
        }
    }
}