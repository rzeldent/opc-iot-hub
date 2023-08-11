using System;
using OpcIotHub.Helpers;

namespace OpcIotHub.Interfaces
{
    public interface ISample
    {
        public DateTime Timestamp { get; }
        public string PropertyName { get; }
        public object Value { get; }
        public SiUnit? Unit { get; }
    }
}
