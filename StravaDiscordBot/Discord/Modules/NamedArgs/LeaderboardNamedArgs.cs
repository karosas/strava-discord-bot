using Discord.Commands;

namespace StravaDiscordBot.Discord.Modules.NamedArgs
{
    // Probably could be expanded later with more optional parameters (e.g. start/end dates)
    [NamedArgumentType]
    public class LeaderboardNamedArgs
    {
        public bool WithRoles { get; set; }
    }
}
