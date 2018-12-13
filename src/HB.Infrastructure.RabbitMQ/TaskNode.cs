using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.RabbitMQ
{
    internal class TaskNode
    {
        public Task Task { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
