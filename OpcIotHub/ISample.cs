using System;

namespace OpcIotHub
{
    public interface ISample
    {
        DateTime Timestamp { get; }
        string PropertyName { get; }
        object Value { get; }
        SiUnit? Unit { get; }
    }
}
