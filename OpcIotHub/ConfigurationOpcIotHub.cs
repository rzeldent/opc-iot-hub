using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpcIotHub.Opc;
using Workstation.ServiceModel.Ua;

namespace OpcIotHub
{
    public class ConfigurationOpcIotHub : IConfigurationOpc, IConfigurationAzureIotHub, IConfigurationMqtt
    {
        private readonly ILogger<ConfigurationOpcIotHub> _logger;
        private readonly IConfiguration _configuration;

        public ConfigurationOpcIotHub(ILogger<ConfigurationOpcIotHub> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public string OpcEndpointUrl => _configuration.GetSection("Opc:Connection")["EndpointUrl"];

        public string OpcUsername => _configuration.GetSection("Opc:Connection")["UserName"];

        public string OpcPassword => _configuration.GetSection("Opc:Connection")["Password"];

        public IEnumerable<OpcNode> MonitorNodes => _configuration.GetSection("Opc:MonitorNodes").GetChildren().Select(c => new OpcNode { NodeId = NodeId.Parse(c["NodeId"]), PropertyName = c["PropertyName"], Unit = c["Unit"].ParseNullable<SiUnit>() });

        public string IotHubConnectionString => _configuration.GetSection("Azure:IotHub")["ConnectionString"];

        public string CaCertificate => _configuration.GetSection("Amazon:IotHub")["CaCertificate"];
        public string ClientCertificate => _configuration.GetSection("Amazon:IotHub")["ClientCertificate"];

        public string ServiceUrl => _configuration.GetSection("Amazon:IotHub")["ServiceUrl"];

        public string Topic => _configuration.GetSection("Amazon:IotHub")["Topic"];
    }
}
