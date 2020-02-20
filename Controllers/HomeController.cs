using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            return Ok($"Bot is Online - {DateTime.UtcNow:u}");
        }
    }
}