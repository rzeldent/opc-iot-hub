using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpcIotHub
{
    public interface ISampleSource : IObservable<ISample>
    {
        Task Publish(CancellationToken token);
    }

}
