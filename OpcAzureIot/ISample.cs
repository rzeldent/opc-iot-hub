using System;

namespace OpcAzureIot
{
    public interface ISample
    {
        DateTime Timestamp { get; }
        string PropertyName { get; }
        object Value { get; }
        SiUnit Unit { get; }
    }
}
