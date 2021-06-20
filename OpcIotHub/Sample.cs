using System;

namespace OpcIotHub
{
    public class Sample : ISample
    {
        public DateTime Timestamp { get; set; }
        public string PropertyName { get; set; }
        public SiUnit? Unit { get; set; }
        public object Value { get; set; }
    }
}
