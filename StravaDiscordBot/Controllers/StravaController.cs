/*using System;
using System.Threading.Tasks;
using IO.Swagger.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            
        }
    }
}*/