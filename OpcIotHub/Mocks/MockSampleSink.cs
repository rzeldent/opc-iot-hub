using System;
using Microsoft.Extensions.Logging;

namespace OpcIotHub.Mocks
{
    public class MockSampleSink : ISampleSink
    {
        private readonly ILogger<MockSampleSink> Logger;

        public MockSampleSink(ILogger<MockSampleSink> logger)
        {
            Logger = logger;
        }

        public void OnCompleted()
        {
            Logger.LogInformation("OnCompleted");
        }

        public void OnError(Exception error)
        {
            Logger.LogError(error, "OnError");
        }

        public void OnNext(ISample value)
        {
            Logger.LogInformation("OnNext: {@Sample}", value);
        }
    }
}
