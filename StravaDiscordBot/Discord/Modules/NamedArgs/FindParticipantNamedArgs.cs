using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace StravaDiscordBot.Discord.Modules.NamedArgs
{
    [NamedArgumentType]
    public class FindParticipantNamedArgs
    {
        public string DiscordId { get; set; }
        public string StravaId { get; set; }
    }
}
