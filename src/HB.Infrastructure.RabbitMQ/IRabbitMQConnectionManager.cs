using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace HB.Infrastructure.RabbitMQ
{
    public interface IRabbitMQConnectionManager : IDisposable
    {
        IModel CreateChannel(string brokerName, bool isPublish);
    }
}
