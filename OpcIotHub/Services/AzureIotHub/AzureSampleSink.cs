using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using OpcIotHub.Interfaces;
using OpcIotHub.Settings;

namespace OpcIotHub.Services.AzureIotHub
{
    public class AzureSampleSink : ISampleSink, IDisposable
    {
        private const TransportType _transportType = TransportType.Mqtt;
        private readonly ILogger<AzureSampleSink> _logger;
        private readonly DeviceClient _client;

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public AzureSampleSink(ILogger<AzureSampleSink> logger, ConfigurationAzureIotHub configuration)
        {
            _logger = logger;
            // Connect to the IoT hub using the MQTT protocol
            _logger.LogInformation("Creating device client to: {IotHubConnectionString}", configuration.IotHubConnectionString);
            _client = DeviceClient.CreateFromConnectionString(configuration.IotHubConnectionString, _transportType);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public async void OnError(Exception error)
        {
            _logger.LogError(error, "OnError");
            await _client?.CloseAsync();
        }

        public async void OnNext(ISample value)
        {
            var messageBody = JsonSerializer.Serialize(value, jsonSerializerOptions);
            using var message = new Message(Encoding.UTF8.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };
            await _client.SendEventAsync(message);
            _logger.LogInformation("Sent message: {Message}", messageBody);
        }

        public async void Dispose()
        {
            _logger.LogInformation("Dispose");
            await _client?.CloseAsync();
        }
    }
}
