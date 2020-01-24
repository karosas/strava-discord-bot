﻿using System.Threading.Tasks;
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

        [HttpGet("callback/{channelId}/{discordUserId}")]
        public async Task<IActionResult> StravaCallback(string channelId, string discordUserId, [FromQuery(Name = "code")] string code, [FromQuery(Name = "scope")] string scope)
        {
            if(!scope.Contains("activity:read"))
                return Ok("Failed to authorize user, read activities permission is needed");

            if(await _stravaService.ParticipantAlreadyExistsAsync(channelId, discordUserId))
                return Ok("It seems this discord user for this channel has already connected to Strava");

            try
            {
                var stravaExchangeResult = await _stravaService.ExchangeCodeAsync(code);
                await _stravaService.CreateLeaderboardParticipantAsync(channelId, discordUserId, stravaExchangeResult);
                return Ok("You are now part of the leaderboard");
            }
            catch (StravaException e)
            {
                return Ok($"Failed to authorize with Strava, error message: {e.Message}");
            }
        }
    }
}