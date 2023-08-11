using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using OpcIotHub.Interfaces;
using OpcIotHub.Settings;

namespace OpcIotHub.Services.Mqtt
{
    public class MqttSampleSink : ISampleSink
    {
        private readonly ILogger<MqttSampleSink> _logger;
        private readonly ConfigurationMqtt _configurationMqtt;

        private IManagedMqttClient _client;

        public MqttSampleSink(ILogger<MqttSampleSink> logger, IOptions<ConfigurationMqtt> configurationMqtt)
        {
            _logger = logger;
            if (configurationMqtt == null)
                throw new ArgumentNullException(nameof(configurationMqtt));

            _configurationMqtt = configurationMqtt.Value;
            Connect();
        }

        private async void Connect()
        {
            var caCertificate = string.IsNullOrEmpty(_configurationMqtt.CaCertificate) ? null : new X509Certificate(Convert.FromBase64String(_configurationMqtt.CaCertificate));
            var clientCertificate = string.IsNullOrEmpty(_configurationMqtt.ClientCertificate) ? null : new X509Certificate(Convert.FromBase64String(_configurationMqtt.ClientCertificate), string.Empty, X509KeyStorageFlags.Exportable);
            _logger.LogInformation("Creating device client to: {ServiceUrl}", _configurationMqtt.ServiceUrl);
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(30))
                .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_configurationMqtt.ServiceUrl, 8883)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                    Certificates = new[] { clientCertificate },
                    CertificateValidationHandler = (ctx) =>
                    {
                        // Accept without validating
                        return true;
                    }
                }).WithProtocolVersion(MqttProtocolVersion.V311)
            )
            .Build();
            _client = new MqttFactory().CreateManagedMqttClient();
            await _client.StartAsync(options);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "OnError");
            _client?.Dispose();
        }

        public async void OnNext(ISample value)
        {
            var json = JsonConvert.SerializeObject(value);
            await _client.EnqueueAsync(new MqttApplicationMessage
            {
                Topic = _configurationMqtt.Topic,
                PayloadSegment = Encoding.UTF8.GetBytes(json),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            });

            _logger.LogInformation("Pusblished: {Message}.", json);
        }
    }
}

