using System;
using OpcIotHub.Helpers;
using OpcIotHub.Interfaces;

namespace OpcIotHub.Model
{
    public class Sample : ISample
    {
        public DateTime Timestamp { get; set; }
        public string PropertyName { get; set; }
        public SiUnit? Unit { get; set; }
        public object Value { get; set; }
    }
}
