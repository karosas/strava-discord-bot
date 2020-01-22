using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public class JoinLeaderboardCommand : CommandBase
    {
        private readonly IStravaService _stravaService;
        private readonly ILogger<JoinLeaderboardCommand> _logger;

        public JoinLeaderboardCommand(IStravaService stravaService, ILogger<JoinLeaderboardCommand> logger)
        {
            _stravaService = stravaService;
            _logger = logger;
        }

        public override string CommandName => "join";
        public override string Descriptions => "Join the leaderboard.";

        public override async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `join`");

            _logger.LogInformation($"Executing 'Join' command. Full: {message.Content} | Author: {message.Author}");

            if(await _stravaService.ParticipantAlreadyExistsAsync(message.Channel.Id.ToString(), message.Author.Id.ToString()))
                throw new InvalidCommandArgumentException($"Whoops, it seems like you're already participating in the leaderboard");

            var dmChannel = await message.Author.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"Hey, {message.Author.Mention} ! Please go to this url to allow me check out your Strava activities: {_stravaService.GetOAuthUrl(message.Channel.Id.ToString(), message.Author.Id.ToString())}");
        }
    }
}
