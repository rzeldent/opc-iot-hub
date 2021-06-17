using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpcAzureIot.Opc;
using Workstation.ServiceModel.Ua;

namespace OpcAzureIot
{
    public class ConfigurationOpcAzureIot : IConfigurationOpc, IConfigurationAzureIotHub
    {
        private readonly ILogger<ConfigurationOpcAzureIot> _logger;
        private readonly IConfiguration _configuration;

        public ConfigurationOpcAzureIot(ILogger<ConfigurationOpcAzureIot> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public string OpcEndpointUrl => _configuration.GetSection("Opc:Connection")["EndpointUrl"];

        public string OpcUsername => _configuration.GetSection("Opc:Connection")["UserName"];

        public string OpcPassword => _configuration.GetSection("Opc:Connection")["Password"];

        public IEnumerable<OpcNode> MonitorNodes => _configuration.GetSection("Opc:MonitorNodes").GetChildren().Select(c => new OpcNode { NodeId = NodeId.Parse(c["NodeId"]), PropertyName=c["PropertyName"], Unit= SiUnitHelpers.Parse(c["Unit"]) });

        public string IotHubConnectionString => _configuration.GetSection("Azure:IotHub")["ConnectionString"];
    }
}
