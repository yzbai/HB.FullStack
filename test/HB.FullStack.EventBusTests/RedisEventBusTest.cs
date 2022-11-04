using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack;
using HB.FullStack.BaseTest;
using HB.FullStack.EventBus.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.Infrastructure.Redis.Test
{
    [TestClass]
    public class RedisEventBusTest : BaseTestClass, IEventHandler
    {
        [TestMethod]
        public async Task TestEventBusAsync()
        {
            string eventName = EventSchemas[0].EventName;

            EventBus.Subscribe(eventName, this);

            EventBus.StartHandle(eventName);

            Thread.Sleep(2 * 1000);

            for (int i = 0; i < 100; ++i)
            {
                await EventBus.PublishAsync(eventName, $"Hello, Just say {i} times.").ConfigureAwait(false);
            }

            Thread.Sleep(1 * 60 * 1000);

            await EventBus.UnSubscribeAsync(eventName).ConfigureAwait(false);
        }

        public Task HandleAsync(string jsonData, CancellationToken cancellationToken)
        {
            Console.WriteLine(jsonData);
            return Task.CompletedTask;
        }
    }
}