using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Discord;
using StravaDiscordBot.Services;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace StravaDiscordBot.Controllers
{
    [Route("strava")]
    [ApiController]
    public class StravaController : ControllerBase
    {
        private readonly ILogger<StravaController> _logger;
        private readonly IStravaService _stravaService;
        public StravaController(ILogger<StravaController> logger, IStravaService stravaService)
        {
            _logger = logger;
            _stravaService = stravaService;
        }

        [HttpGet("callback/{serverId}/{discordUserId}")]
        public async Task<IActionResult> StravaCallback(string serverId, string discordUserId, [FromQuery(Name = "code")] string code, [FromQuery(Name = "scope")] string scope)
        {
            if (scope == null || !scope.Contains("activity:read", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation($"Insufficient scopes for {discordUserId}");
                return Ok("Failed to authorize user, read activities permission is needed");
            }

            try
            {
                await _stravaService.ExchangeCodeAndCreateOrRefreshParticipant(serverId, discordUserId, code).ConfigureAwait(false);
                return Ok("You are now part of the leaderboard");
            }
            catch (StravaException e)
            {
                _logger.LogError(e, "Failed to authorize with strava");
                return Ok($"Failed to authorize with Strava, error message: {e.Message}");
            }
            catch(InvalidCommandArgumentException e)
            {
                _logger.LogError(e, "Failed to create user with unknown error");
                return Ok(e.Message);
            }
        }
    }
}
