using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace OpcIotHub.Opc
{
    public class OpcSampleSource : ISampleSource
    {
        const string ApplicationName = nameof(OpcIotHub);

        private readonly ILogger<OpcSampleSource> Logger;
        private readonly IConfigurationOpc Configuration;

        static readonly ApplicationDescription clientDescription = new ApplicationDescription
        {
            ApplicationName = ApplicationName,
            ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:{ApplicationName}",
            ApplicationType = ApplicationType.Client
        };

        private ISubject<ISample> _samples = new Subject<ISample>();

        public IDisposable Subscribe(IObserver<ISample> observer)
        {
            return _samples.Subscribe(observer);
        }

        public OpcSampleSource(ILogger<OpcSampleSource> logger, IConfigurationOpc configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

       public async Task Publish(CancellationToken token = default)
        {
            // Create a mapping ClientHandle to Node
            var lastMonitorClientHandle = 0U;
            var handleToOpcNode = new Dictionary<uint, OpcNode>();

            // Create a directory for the Certificate store
            var certificateStore = new DirectoryStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationName, "pki"));
            // create a 'UaTcpSessionChannel', a client-side channel that opens a 'session' with the server.
            var userIdentity = string.IsNullOrEmpty(Configuration.OpcUsername) ? new AnonymousIdentity() : (IUserIdentity)new UserNameIdentity(Configuration.OpcUsername, Configuration.OpcPassword);

            while (!token.IsCancellationRequested)
            {
                var channel = new UaTcpSessionChannel(clientDescription, certificateStore, userIdentity, Configuration.OpcEndpointUrl, SecurityPolicyUris.None);
                try
                {
                    // try opening a session and reading a few nodes.
                    await channel.OpenAsync(token);

                    Logger.LogDebug("Opened session with endpoint {@EndpointUrl}", channel.RemoteEndpoint.EndpointUrl);
                    Logger.LogDebug("SecurityPolicy: {SecurityPolicy}", channel.RemoteEndpoint.SecurityPolicyUri);
                    Logger.LogDebug("SecurityMode: {SecurityMode}", channel.RemoteEndpoint.SecurityMode);
                    Logger.LogDebug("UserIdentityToken: {@UserIdentity}", channel.UserIdentity);

                    var opcSubscriptionResponse = await channel.CreateSubscriptionAsync(new CreateSubscriptionRequest
                    {
                        RequestedPublishingInterval = 100,
                        RequestedMaxKeepAliveCount = 10,
                        RequestedLifetimeCount = 30,
                        PublishingEnabled = true
                    }, token);
                    if (StatusCode.IsBad(opcSubscriptionResponse.ResponseHeader.ServiceResult))
                    {
                        Logger.LogError("CreateSubscriptionAsync failed. ServiceResult:{ServiceResult}", opcSubscriptionResponse.ResponseHeader.ServiceResult);
                        return;
                    }

                    var subscriptionId = opcSubscriptionResponse.SubscriptionId;

                    var monitorNodes = Configuration.MonitorNodes.ToArray();
                    var itemsToCreate = monitorNodes.Select(n =>
                    {
                        var clientHandle = ++lastMonitorClientHandle;
                        handleToOpcNode[clientHandle] = n;
                        Logger.LogDebug("Mapped handle {ClientHandle} to '{@OpcNode}'", clientHandle, n);

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
                    });

                    if (StatusCode.IsBad(createMonitoredItemsResponse.ResponseHeader.ServiceResult))
                    {
                        Logger.LogError("CreateMonitoredItemsAsync failed. ServiceResult:{ServiceResult}", createMonitoredItemsResponse.ResponseHeader.ServiceResult);
                        return;
                    }

                    for (var i = 0; i < createMonitoredItemsResponse.Results.Length; ++i)
                    {
                        var statusCode = createMonitoredItemsResponse.Results[i].StatusCode;
                        if (StatusCode.IsBad(statusCode))
                            Logger.LogWarning("Failed to monitor '{@OpcNode}'. StatusCode: {StatusCode}", monitorNodes[i], statusCode);
                    };

                    var subscription = channel.Where(pr => pr.SubscriptionId == subscriptionId).Subscribe(pr =>
                        {
                            Logger.LogInformation("Received update for SubscriptionId: {SubscriptionId}", pr.SubscriptionId);
                            var dcns = pr.NotificationMessage.NotificationData.OfType<DataChangeNotification>();
                            foreach (var dcn in dcns)
                            {
                                foreach (var mi in dcn.MonitoredItems)
                                {
                                    if (!handleToOpcNode.TryGetValue(mi.ClientHandle, out var opcNode))
                                    {
                                        Logger.LogError("Unknown ClientHandle: {ClientHandle}", mi.ClientHandle);
                                        return;
                                    }

                                    var sample = new Sample
                                    {
                                        Timestamp = mi.Value.SourceTimestamp,
                                        PropertyName = opcNode.PropertyName ?? opcNode.NodeId.ToString(),
                                        Value = mi.Value.Value,
                                        Unit = opcNode.Unit
                                    };
                                    Logger.LogInformation("Created sample: {@Sample}", sample);

                                    // Publish the sample
                                    _samples.OnNext(sample);
                                }
                            }
                        },
                        // need to handle error when server closes
                        ex => Logger.LogError(ex, "Error subscription on channel"));

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

                    Logger.LogDebug("Deleting subscription {SubscriptionId}", subscriptionId);
                    await channel.DeleteSubscriptionsAsync(request, token);
                    subscription.Dispose();

                    Logger.LogDebug("Closing session {SessionId}", channel.SessionId);
                    await channel.CloseAsync(token);

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Exception setting up OPC connection");
                    await channel.AbortAsync(token);
                    _samples.OnError(ex);
                    try
                    {
                        await Task.Delay(5000, token);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }
        }
    }
}
