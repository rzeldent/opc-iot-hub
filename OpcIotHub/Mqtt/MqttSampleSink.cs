using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace OpcIotHub.Mqtt
{
    public class MqttSampleSink : ISampleSink
    {
        private readonly ILogger<MqttSampleSink> _logger;
        private readonly IConfigurationMqtt _configuration;

        private IManagedMqttClient _client;

        public MqttSampleSink(ILogger<MqttSampleSink> logger, IConfigurationMqtt configuration)
        {
            _logger = logger;
            _configuration = configuration;
            Connect();
        }

        private async void Connect()
        {
            var caCertificate = string.IsNullOrEmpty(_configuration.CaCertificate) ? null : new X509Certificate(Convert.FromBase64String(_configuration.CaCertificate));
            var clientCertificate = string.IsNullOrEmpty(_configuration.ClientCertificate) ? null : new X509Certificate(Convert.FromBase64String(_configuration.ClientCertificate), string.Empty, X509KeyStorageFlags.Exportable);
            _logger.LogInformation("Creating device client to: {0}", _configuration.ServiceUrl);
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(30))
                .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(_configuration.ServiceUrl, 8883)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                    {
                        UseTls = true,
                        SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                        Certificates = new[]
                        {
                            /*caCertificate, */clientCertificate
                        },

                        CertificateValidationHandler = (MqttClientCertificateValidationCallbackContext ctx) =>
                        {
                            return true;
                        }
                    }
                    )
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                )
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            
            _client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(onSubscriberConnected =>
            {
                _logger.LogInformation("OnConnected:{@}", onSubscriberConnected);
            });
            _client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(onSubscriberDisconnected => {
                _logger.LogInformation("OnDisonnected");
            });
            _client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(onSubscriberMessageReceived=>
            {
                _logger.LogInformation("MessageReceived");
            });
            
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
            string json = JsonConvert.SerializeObject(value);
            var result = await _client.PublishAsync(new MqttApplicationMessage
            {
                Topic = _configuration.Topic,
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                Payload = Encoding.UTF8.GetBytes(json),
            });

            _logger.LogInformation("Pusblished: {message}. Result: {@result}", json, result);
        }
    }
}

