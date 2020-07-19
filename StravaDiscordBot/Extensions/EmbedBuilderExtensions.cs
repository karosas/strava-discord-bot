using System;
using Discord;

namespace StravaDiscordBot.Extensions
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder AddField(this EmbedBuilder builder, string title, object value, bool inline = false)
        {
            try
            {
                builder
                    .AddField(efb =>
                        efb
                            .WithName(title)
                            .WithValue(value)
                            .WithIsInline(inline)
                    );
            }
            catch (Exception)
            {
                // I'd rather have this silent catch-all,
                // than exception while adding a field causing command fail to execute
            }

            return builder;
        }
    }
}