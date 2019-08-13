using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace StravaDiscordBot.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void UseDiscordSocketCommandContextModule<T>(this IApplicationBuilder applicationBuilder) where T : ModuleBase<SocketCommandContext>
        {
            var commandService = applicationBuilder.ApplicationServices.GetService<CommandService>();
            commandService.AddModulesAsync(Assembly.GetEntryAssembly(), applicationBuilder.ApplicationServices)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
