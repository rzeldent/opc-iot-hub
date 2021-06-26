using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace OpcIotHub.AzureIotHub
{
    public class AzureSampleSink : ISampleSink, IDisposable
    {
        private readonly ILogger<AzureSampleSink> _logger;

        private DeviceClient _deviceClient;
        private const TransportType _transportType = TransportType.Mqtt;

        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public AzureSampleSink(ILogger<AzureSampleSink> logger, IConfigurationAzureIotHub configuration)
        {
            _logger = logger;

            // Connect to the IoT hub using the MQTT protocol
            _logger.LogInformation("Creating device client to: {0}", configuration.IotHubConnectionString);
            _deviceClient = DeviceClient.CreateFromConnectionString(configuration.IotHubConnectionString, _transportType);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public async void OnError(Exception error)
        {
            _logger.LogError(error, "OnError");
            await _deviceClient?.CloseAsync();
        }

        public async void OnNext(ISample value)
        {
            string messageBody = JsonSerializer.Serialize(value, jsonSerializerOptions);
            using var message = new Message(Encoding.UTF8.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };
            await _deviceClient.SendEventAsync(message);
            _logger.LogInformation("Sent message: {Message}", messageBody);
        }

        public async void Dispose()
        {
            _logger.LogInformation("Dispose");
            await _deviceClient?.CloseAsync();
        }
    }
}
