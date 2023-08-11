using OpcIotHub.Services.Opc;
using System.Collections.Generic;

namespace OpcIotHub.Settings
{
    public class ConfigurationOpc
    {
        public const string Section = "Opc:Connecion";

        // Opc:Connection
        public string OpcEndpointUrl { get; set; }
        public string OpcUsername { get; set; }
        public string OpcPassword { get; set; }
        // Opc:Nodes
        public IEnumerable<OpcNode> MonitorNodes { get; set; }
    }
}
