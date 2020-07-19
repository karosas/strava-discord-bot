using System;
using System.Globalization;
using System.Threading.Tasks;
using IO.Swagger.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Controllers
{
    [Route("strava")]
    [ApiController]
    public class StravaController : ControllerBase
    {
        private readonly ILogger<StravaController> _logger;
        private readonly IStravaAuthenticationService _stravaAuthenticationService;
        private readonly IAthleteService _athleteSrvice;
        private readonly ILeaderboardParticipantService _leaderboardParticipantService;
        private readonly IStravaCredentialService _credentialService;

        public StravaController(ILogger<StravaController> logger,
            IStravaAuthenticationService stravaAuthenticationService,
            IAthleteService athleteSrvice,
            ILeaderboardParticipantService leaderboardParticipantService,
            IStravaCredentialService credentialService)
        {
            _logger = logger;
            _stravaAuthenticationService = stravaAuthenticationService;
            _athleteSrvice = athleteSrvice;
            _leaderboardParticipantService = leaderboardParticipantService;
            _credentialService = credentialService;
        }

        [HttpGet("callback/{serverId}/{discordUserId}")]
        public async Task<IActionResult> StravaCallback(string serverId, string discordUserId,
            [FromQuery(Name = "code")] string code, [FromQuery(Name = "scope")] string scope)
        {
            if (scope == null || !scope.Contains("activity:read", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation($"Insufficient scopes for {discordUserId}");
                return Ok("Failed to authorize user, read activities permission is needed");
            }

            try
            {
                var exchangeResult = await _stravaAuthenticationService.ExchangeCodeAsync(code);
                var athlete = await _athleteSrvice.Get(null, exchangeResult.AccessToken);
                var participant = _leaderboardParticipantService.GetParticipantOrDefault(serverId, discordUserId);

                if (participant == null)
                    await _leaderboardParticipantService.CreateWithCredentials(new LeaderboardParticipant(serverId, discordUserId, athlete.Id.ToString()), exchangeResult);

                await _credentialService.UpsertTokens(athlete.Id.ToString(), exchangeResult);
                return Ok("You are now part of the leaderboard");
            }
            catch (ApiException e)
            {
                _logger.LogError(e, "Failed to authorize with strava");
                return Ok($"Failed to authorize with Strava, error message: {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create user with unknown error");
                return Ok(e.Message);
            }
        }
    }
}