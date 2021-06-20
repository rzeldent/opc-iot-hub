using Workstation.ServiceModel.Ua;

namespace OpcIotHub.Opc
{
    public struct OpcNode
    {
        public string PropertyName { get; set; }
        public SiUnit? Unit { get; set; }
        public NodeId NodeId { get; set; }
    }
}
