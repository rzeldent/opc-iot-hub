using System.Collections.Generic;

namespace OpcIotHub
{
    public interface IConfigurationOpc
    {
        // Opc:Connection
        public string OpcEndpointUrl { get; }
        public string OpcUsername { get; }
        public string OpcPassword { get; }
        // Opc:Nodes
        IEnumerable<Opc.OpcNode> MonitorNodes { get; }
    }
}
