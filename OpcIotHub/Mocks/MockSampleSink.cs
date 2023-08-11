using System;
using Microsoft.Extensions.Logging;
using OpcIotHub.Interfaces;

namespace OpcIotHub.Mocks
{
    public class MockSampleSink : ISampleSink
    {
        private readonly ILogger<MockSampleSink> _logger;

        public MockSampleSink(ILogger<MockSampleSink> logger)
        {
            _logger = logger;
        }

        public void OnCompleted()
        {
            _logger.LogInformation("OnCompleted");
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "OnError");
        }

        public void OnNext(ISample value)
        {
            _logger.LogInformation("OnNext: {@Sample}", value);
        }
    }
}
