using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpcIotHub.Mocks
{
    public class MockSampleSource : ISampleSource
    {
        private readonly ILogger<MockSampleSource> _logger;

        private readonly IConnectableObservable<ISample> _samples;

        public MockSampleSource(ILogger<MockSampleSource> logger)
        {
            _logger = logger;

            _samples = Observable
                .Interval(TimeSpan.FromMilliseconds(2500))
                .Select(_ => new Sample
                {
                    PropertyName = "Test",
                    Unit = SiUnit.Pressure,
                    Timestamp = DateTime.UtcNow,
                    Value = new Random().NextDouble() * 10
                }
                )
                .Do(s => _logger.LogInformation("Generated new Sample: {@Sample}", s))
                .Publish();
        }

        public IDisposable Subscribe(IObserver<ISample> observer)
        {
            return _samples.Subscribe(observer);
        }

        public async Task Publish(CancellationToken token = default)
        {
            await Task.Run(() =>
            {
                var subscription = _samples.Connect();
                token.WaitHandle.WaitOne();
                subscription.Dispose();
            }, token);
        }
    }
}
