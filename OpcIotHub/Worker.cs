using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpcIotHub.Interfaces;

// See: https://dotnetcoretutorials.com/2019/12/07/creating-windows-services-in-net-core-part-3-the-net-core-worker-way/

namespace OpcIotHub
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

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            var subscription = _source.Subscribe(_sink);
            var publishTask = _source.Publish(stopToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!stopToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running for: {elapsed}", stopwatch.Elapsed);
                await Task.Delay(10000, stopToken);
            }

            publishTask.GetAwaiter().GetResult();

            subscription.Dispose();
        }
    }
}
