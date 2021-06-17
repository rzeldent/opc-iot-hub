using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// See: https://dotnetcoretutorials.com/2019/12/07/creating-windows-services-in-net-core-part-3-the-net-core-worker-way/

namespace OpcAzureIot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISampleSink _sink;
        private readonly ISampleSource _source;

        public Worker(ILogger<Worker> logger, ISampleSink sink, ISampleSource source)
        {
            _logger = logger;
            _sink = sink;
            _source = source;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscription = _source.Subscribe(_sink);

            var publishTask = _source.Publish(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            publishTask.GetAwaiter().GetResult();

            subscription.Dispose();


        }
    }
}
