using System;

namespace OpcIotHub
{
    public interface ISample
    {
        public DateTime Timestamp { get; }
        public string PropertyName { get; }
        public object Value { get; }
        public SiUnit? Unit { get; }
    }
}
