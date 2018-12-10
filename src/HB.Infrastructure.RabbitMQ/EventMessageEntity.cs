using HB.Framework.Common;
using HB.Framework.EventBus.Abstractions;
using HB.Framework.KVStore.Entity;

namespace HB.Infrastructure.RabbitMQ
{
    public class EventMessageEntity : KVStoreEntity
    {
        [KVStoreKey]
        public string Id { get; set; }

        public string BrokerName { get; set; }

        public EventMessage Message { get; set; }

        public EventMessageEntity() { }

        public EventMessageEntity(string brokerName, EventMessage message) : this()
        {
            Id = SecurityHelper.CreateUniqueToken();
            BrokerName = brokerName;
            Message = message;
        }
    }
}
