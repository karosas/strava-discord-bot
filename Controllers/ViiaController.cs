using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Discord;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace StravaDiscordBot.Controllers
{
    [Route("strava")]
    [ApiController]
    public class ViiaController : ControllerBase
    {
        private readonly IStravaService _stravaService;
        public ViiaController(IStravaService stravaService)
        {
            _stravaService = stravaService;
        }

        [HttpGet("callback/{serverId}/{discordUserId}")]
        public async Task<IActionResult> StravaCallback(string serverId, string discordUserId, [FromQuery(Name = "code")] string code, [FromQuery(Name = "scope")] string scope)
        {
            if(scope == null || !scope.Contains("activity:read", StringComparison.InvariantCultureIgnoreCase))
                return Ok("Failed to authorize user, read activities permission is needed");

            try
            {
                await _stravaService.ExchangeCodeAndCreateOrRefreshParticipant(serverId, discordUserId, code).ConfigureAwait(false);
                return Ok("You are now part of the leaderboard");
            }
            catch (StravaException e)
            {
                return Ok($"Failed to authorize with Strava, error message: {e.Message}");
            }
            catch(InvalidCommandArgumentException e)
            {
                return Ok(e.Message);
            }
        }
    }
}
