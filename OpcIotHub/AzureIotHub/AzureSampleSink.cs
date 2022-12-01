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
        private readonly ILogger<AzureSampleSink> Logger;
        private const TransportType _transportType = TransportType.Mqtt;
        private DeviceClient _client;

        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public AzureSampleSink(ILogger<AzureSampleSink> logger, IConfigurationAzureIotHub configuration)
        {
            Logger = logger;
            // Connect to the IoT hub using the MQTT protocol
            Logger.LogInformation("Creating device client to: {0}", configuration.IotHubConnectionString);
            _client = DeviceClient.CreateFromConnectionString(configuration.IotHubConnectionString, _transportType);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public async void OnError(Exception error)
        {
            Logger.LogError(error, "OnError");
            await _client?.CloseAsync();
        }

        public async void OnNext(ISample value)
        {
            string messageBody = JsonSerializer.Serialize(value, jsonSerializerOptions);
            using var message = new Message(Encoding.UTF8.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };
            await _client.SendEventAsync(message);
            Logger.LogInformation("Sent message: {Message}", messageBody);
        }

        public async void Dispose()
        {
            Logger.LogInformation("Dispose");
            await _client?.CloseAsync();
        }
    }
}
