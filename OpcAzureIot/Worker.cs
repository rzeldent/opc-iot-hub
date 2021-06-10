using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// See: https://dotnetcoretutorials.com/2019/12/07/creating-windows-services-in-net-core-part-3-the-net-core-worker-way/

namespace OpcAzureIot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IOpcSource _opcSource;

        public Worker(ILogger<Worker> logger, IOpcSource opcSource)
        {
            _logger = logger;
            _opcSource = opcSource;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
