using Workstation.ServiceModel.Ua;

namespace OpcAzureIot.Opc
{
    public struct OpcNode
    {
        public string PropertyName { get; set; }
        public SiUnit Unit { get; set; }
        public NodeId NodeId { get; set; }
    }
}
