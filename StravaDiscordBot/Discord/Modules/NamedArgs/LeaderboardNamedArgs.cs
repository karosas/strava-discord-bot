using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
