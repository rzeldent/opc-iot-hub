using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// See: https://dotnetcoretutorials.com/2019/12/07/creating-windows-services-in-net-core-part-3-the-net-core-worker-way/

namespace OpcIotHub
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> Logger;
        private readonly ISampleSink Sink;
        private readonly ISampleSource Source;

        public Worker(ILogger<Worker> logger, ISampleSink sink, ISampleSource source)
        {
            Logger = logger;
            Sink = sink;
            Source = source;
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            var subscription = Source.Subscribe(Sink);
            var publishTask = Source.Publish(stopToken);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!stopToken.IsCancellationRequested)
            {
                Logger.LogInformation("Worker running for: {elapsed}", stopwatch.Elapsed);
                await Task.Delay(10000, stopToken);
            }

            publishTask.GetAwaiter().GetResult();

            subscription.Dispose();
        }
    }
}
