using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot.Extensions
{
    public static class DateTimeExtensions
    {
        public static long GetEpochTimestamp(this DateTime datetime)
        {
            return (long) (datetime - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
