using System;
using Microsoft.Extensions.Logging;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace OpcAzureIot
{
    public class OpcSource : IOpcSource
    {
        private readonly ILogger<OpcSource> _logger;
        

        static readonly ApplicationDescription clientDescription = new ApplicationDescription
        {
            ApplicationName = "Workstation.UaClient.FeatureTests",
            ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:Workstation.UaClient.FeatureTests",
            ApplicationType = ApplicationType.Client
        };

        public OpcSource(ILogger<OpcSource> logger)
        {
            _logger = logger;

            ConnectOpc();
        }

        private  async void ConnectOpc()
        {
            // create a 'UaTcpSessionChannel', a client-side channel that opens a 'session' with the server.
            var channel = new UaTcpSessionChannel(
                clientDescription,
                null, // no x509 certificates
                new AnonymousIdentity(), // no user identity
                "opc.tcp://opcua.rocks:4840", // the public endpoint of a server at opcua.rocks.
                SecurityPolicyUris.None); // no encryption
            try
            {
                // try opening a session and reading a few nodes.
                await channel.OpenAsync();

                _logger.LogDebug($"Opened session with endpoint '{channel.RemoteEndpoint.EndpointUrl}'.");
                _logger.LogDebug($"SecurityPolicy: '{channel.RemoteEndpoint.SecurityPolicyUri}'.");
                _logger.LogDebug($"SecurityMode: '{channel.RemoteEndpoint.SecurityMode}'.");
                _logger.LogDebug($"UserIdentityToken: '{channel.UserIdentity}'.");

                // build a ReadRequest. See 'OPC UA Spec Part 4' paragraph 5.10.2
                var readRequest = new ReadRequest
                {
                    // set the NodesToRead to an array of ReadValueIds.
                    NodesToRead = new[] {
                    // construct a ReadValueId from a NodeId and AttributeId.
                    new ReadValueId {
                        // you can parse the nodeId from a string.
                        // e.g. NodeId.Parse("ns=2;s=Demo.Static.Scalar.Double")
                        NodeId = NodeId.Parse(VariableIds.Server_ServerStatus),
                        // variable class nodes have a Value attribute.
                        AttributeId = AttributeIds.Value
                    }
                }
                };
                // send the ReadRequest to the server.
                var readResult = await channel.ReadAsync(readRequest);

                // DataValue is a class containing value, timestamps and status code.
                // the 'Results' array returns DataValues, one for every ReadValueId.
                var serverStatus = readResult.Results[0].GetValueOrDefault<ServerStatusDataType>();

                _logger.LogInformation("ProductName: {0}", serverStatus.BuildInfo.ProductName);
                _logger.LogInformation("SoftwareVersion: {0}", serverStatus.BuildInfo.SoftwareVersion);
                _logger.LogInformation("ManufacturerName: {0}", serverStatus.BuildInfo.ManufacturerName);
                _logger.LogInformation("State: {0}", serverStatus.State);
                _logger.LogInformation("CurrentTime: {0}", serverStatus.CurrentTime);

                _logger.LogDebug($"\nClosing session '{channel.SessionId}'.");
                await channel.CloseAsync();
            }
            catch (Exception ex)
            {
                await channel.AbortAsync();
                _logger.LogError(ex.Message);
            }
        }
    }
}
