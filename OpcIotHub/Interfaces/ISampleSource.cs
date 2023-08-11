using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpcIotHub.Interfaces
{
    public interface ISampleSource : IObservable<ISample>
    {
        public Task Publish(CancellationToken token);
    }
}
