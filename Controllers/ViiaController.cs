using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Services;
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

        [HttpGet("callback/{serverId}/{channelId}/{discordUserId}")]
        public async Task<IActionResult> StravaCallback(string serverId, string discordUserId, [FromQuery(Name = "code")] string code, [FromQuery(Name = "scope")] string scope)
        {
            if(scope == null || !scope.Contains("activity:read", StringComparison.InvariantCultureIgnoreCase))
                return Ok("Failed to authorize user, read activities permission is needed");

            if(await _stravaService.DoesParticipantAlreadyExistsAsync(serverId, discordUserId).ConfigureAwait(false))
                return Ok("It seems this discord user for this channel has already connected to Strava");

            try
            {
                await _stravaService.ExchangeCodeAndCreateParticipant(serverId, discordUserId, code).ConfigureAwait(false);
                return Ok("You are now part of the leaderboard");
            }
            catch (StravaException e)
            {
                return Ok($"Failed to authorize with Strava, error message: {e.Message}");
            }
        }
    }
}
