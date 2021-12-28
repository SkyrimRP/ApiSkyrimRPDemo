using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApiSkyrimRP.Core
{
    public class ServersCache : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ServersCacheService service;

        public ServersCache(ServersCacheService _service)
        {
            service = _service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
            return Task.CompletedTask;
        }
        private void DoWork(object state)
        {
            service.RefreshServers(DateTime.Now);
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
