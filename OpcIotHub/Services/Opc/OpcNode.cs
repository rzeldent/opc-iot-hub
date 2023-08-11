using OpcIotHub.Helpers;
using Workstation.ServiceModel.Ua;

namespace OpcIotHub.Services.Opc
{
    public struct OpcNode
    {
        public string PropertyName { get; set; }
        public SiUnit? Unit { get; set; }
        public NodeId NodeId { get; set; }
    }
}
