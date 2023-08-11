using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpcIotHub.Interfaces;
using OpcIotHub.Model;
using OpcIotHub.Settings;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace OpcIotHub.Services.Opc
{
    public class OpcSampleSource : ISampleSource
    {
        private readonly ILogger<OpcSampleSource> _logger;
        private readonly ConfigurationOpc _configurationOpc;
        private readonly string  _applicationName;

        private readonly ISubject<ISample> _samples = new Subject<ISample>();

        public IDisposable Subscribe(IObserver<ISample> observer)
        {
            return _samples.Subscribe(observer);
        }

        public OpcSampleSource(ILogger<OpcSampleSource> logger, IHostEnvironment environment, IOptions<ConfigurationOpc> configurationOpc)
        {
            _logger = logger;
            if (configurationOpc == null)
                throw new ArgumentNullException(nameof(configurationOpc));

            _configurationOpc = configurationOpc.Value;

            _applicationName = environment.ApplicationName;
        }

        public async Task Publish(CancellationToken token = default)
        {
            // Create a mapping ClientHandle to Node
            var lastMonitorClientHandle = 0U;
            var handleToOpcNode = new Dictionary<uint, OpcNode>();

            var applicationDescription = new ApplicationDescription()
            {
                ApplicationName = _applicationName,
                ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:{_applicationName}",
                ApplicationType = ApplicationType.Client
            };
            // Create a directory for the Certificate store
            var certificateStore = new DirectoryStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _applicationName, "pki"));
            // create a 'UaTcpSessionChannel', a client-side channel that opens a 'session' with the server.
            var userIdentity = string.IsNullOrEmpty(_configurationOpc.OpcUsername) ? new AnonymousIdentity() : (IUserIdentity)new UserNameIdentity(_configurationOpc.OpcUsername, _configurationOpc.OpcPassword);

            while (!token.IsCancellationRequested)
            {
                var channel = new ClientSessionChannel(applicationDescription, certificateStore, userIdentity, _configurationOpc.OpcEndpointUrl, SecurityPolicyUris.None);
                try
                {
                    // try opening a session and reading a few nodes.
                    await channel.OpenAsync(token);

                    _logger.LogDebug("Opened session with endpoint {@EndpointUrl}", channel.RemoteEndpoint.EndpointUrl);
                    _logger.LogDebug("SecurityPolicy: {SecurityPolicy}", channel.RemoteEndpoint.SecurityPolicyUri);
                    _logger.LogDebug("SecurityMode: {SecurityMode}", channel.RemoteEndpoint.SecurityMode);
                    _logger.LogDebug("UserIdentityToken: {@UserIdentity}", channel.UserIdentity);

                    var opcSubscriptionResponse = await channel.CreateSubscriptionAsync(new CreateSubscriptionRequest
                    {
                        RequestedPublishingInterval = 100,
                        RequestedMaxKeepAliveCount = 10,
                        RequestedLifetimeCount = 30,
                        PublishingEnabled = true
                    }, token);
                    if (StatusCode.IsBad(opcSubscriptionResponse.ResponseHeader.ServiceResult))
                    {
                        _logger.LogError("CreateSubscriptionAsync failed. ServiceResult:{ServiceResult}", opcSubscriptionResponse.ResponseHeader.ServiceResult);
                        return;
                    }

                    var subscriptionId = opcSubscriptionResponse.SubscriptionId;

                    var monitorNodes = _configurationOpc.MonitorNodes.ToArray();
                    var itemsToCreate = monitorNodes.Select(n =>
                    {
                        var clientHandle = ++lastMonitorClientHandle;
                        handleToOpcNode[clientHandle] = n;
                        _logger.LogDebug("Mapped handle {ClientHandle} to '{@OpcNode}'", clientHandle, n);

                        // Start monitoring the node. Mode = Reporting (on change)
                        return new MonitoredItemCreateRequest
                        {
                            ItemToMonitor = new ReadValueId
                            {
                                NodeId = n.NodeId,
                                AttributeId = AttributeIds.Value
                            },
                            MonitoringMode = MonitoringMode.Reporting,
                            RequestedParameters = new MonitoringParameters
                            {
                                ClientHandle = clientHandle,
                                SamplingInterval = -1,
                                QueueSize = 0,
                                DiscardOldest = true
                            }
                        };
                    }).ToArray();

                    var createMonitoredItemsResponse = await channel.CreateMonitoredItemsAsync(new CreateMonitoredItemsRequest
                    {
                        SubscriptionId = subscriptionId,
                        ItemsToCreate = itemsToCreate
                    }, token);

                    if (StatusCode.IsBad(createMonitoredItemsResponse.ResponseHeader.ServiceResult))
                    {
                        _logger.LogError("CreateMonitoredItemsAsync failed. ServiceResult:{ServiceResult}", createMonitoredItemsResponse.ResponseHeader.ServiceResult);
                        return;
                    }

                    for (var i = 0; i < createMonitoredItemsResponse.Results.Length; ++i)
                    {
                        var statusCode = createMonitoredItemsResponse.Results[i].StatusCode;
                        if (StatusCode.IsBad(statusCode))
                            _logger.LogWarning("Failed to monitor '{@OpcNode}'. StatusCode: {StatusCode}", monitorNodes[i], statusCode);
                    };

                    var subscription = channel.Where(pr => pr.SubscriptionId == subscriptionId).Subscribe(pr =>
                        {
                            _logger.LogInformation("Received update for SubscriptionId: {SubscriptionId}", pr.SubscriptionId);
                            var dcns = pr.NotificationMessage.NotificationData.OfType<DataChangeNotification>();
                            foreach (var dcn in dcns)
                            {
                                foreach (var mi in dcn.MonitoredItems)
                                {
                                    if (!handleToOpcNode.TryGetValue(mi.ClientHandle, out var opcNode))
                                    {
                                        _logger.LogError("Unknown ClientHandle: {ClientHandle}", mi.ClientHandle);
                                        return;
                                    }

                                    var sample = new Sample
                                    {
                                        Timestamp = mi.Value.SourceTimestamp,
                                        PropertyName = opcNode.PropertyName ?? opcNode.NodeId.ToString(),
                                        Value = mi.Value.Value,
                                        Unit = opcNode.Unit
                                    };
                                    _logger.LogInformation("Created sample: {@Sample}", sample);

                                    // Publish the sample
                                    _samples.OnNext(sample);
                                }
                            }
                        },
                        // need to handle error when server closes
                        ex => _logger.LogError(ex, "Error subscription on channel"));

                    try
                    {
                        Task.WaitAny(new Task[] { channel.Completion }, token);
                    }
                    catch (OperationCanceledException)
                    {
                    }

                    var request = new DeleteSubscriptionsRequest
                    {
                        SubscriptionIds = new uint[] { subscriptionId }
                    };

                    _logger.LogDebug("Deleting subscription {SubscriptionId}", subscriptionId);
                    await channel.DeleteSubscriptionsAsync(request, token);
                    subscription.Dispose();

                    _logger.LogDebug("Closing session {SessionId}", channel.SessionId);
                    await channel.CloseAsync(token);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception setting up OPC connection");
                    await channel.AbortAsync(token);
                    _samples.OnError(ex);
                    try
                    {
                        await Task.Delay(5000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogError(ex, "Unable to stop setting up OPC connection");
                    }
                }
            }
        }
    }
}
