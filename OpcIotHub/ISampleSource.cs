using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpcIotHub
{
    public interface ISampleSource : IObservable<ISample>
    {
        public Task Publish(CancellationToken token);
    }

}
