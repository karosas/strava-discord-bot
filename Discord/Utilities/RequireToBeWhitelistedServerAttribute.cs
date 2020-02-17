using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using StravaDiscordBot.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace StravaDiscordBot.Discord.Utilities
{
    public class RequireToBeWhitelistedServerAttribute : PreconditionAttribute
    {
        public RequireToBeWhitelistedServerAttribute() { }

        // Override the CheckPermissions method
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var dbContext = services.GetService<BotDbContext>();
            var serverId = context?.Guild?.Id;
            if(serverId == null)
            {
                Console.WriteLine("ServerId null");
                return Task.FromResult(PreconditionResult.FromError("Not a whitelisted server"));
            }
            var result = dbContext.Leaderboards.Any(x => x.ServerId == serverId.ToString()) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Not a whitelisted server");
            Console.WriteLine($"whitelited seerver requirements success - {result.IsSuccess}");
            return Task.FromResult(result);
        }
    }
}
