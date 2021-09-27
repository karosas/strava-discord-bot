using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;
using Timer = System.Timers.Timer;

namespace StravaDiscordBot.Services.HostedService
{
    public abstract class CronHostedServiceBase : IHostedService, IDisposable
    {
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZoneInfo;
        private Timer _timer;

        public CronHostedServiceBase(string cronExpression, TimeZoneInfo timeZoneInfo)
        {
            _expression = CronExpression.Parse(cronExpression);
            _timeZoneInfo = timeZoneInfo;
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleJob(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                _timer = new Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Stop(); // reset timer
                    await DoWork(cancellationToken).ConfigureAwait(false);
                    await ScheduleJob(cancellationToken).ConfigureAwait(false); // reschedule next
                };
                _timer.Start();
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        protected abstract Task DoWork(CancellationToken cancellationToken);
    }
}