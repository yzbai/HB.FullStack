using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HB.Framework.EventBus.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace HB.Infrastructure.Redis.Test
{
    public class RedisEventBusTest : IClassFixture<ServiceFixture>, IEventHandler
    {
        private readonly ITestOutputHelper _output;
        private readonly IEventBus _eventBus;

        public RedisEventBusTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            _output = testOutputHelper;
            _eventBus = serviceFixture.ThrowIfNull(nameof(serviceFixture)).EventBus;
        }

        public void Handle(string jsonData)
        {
            _output.WriteLine(jsonData);
        }

        [Fact]
        public void TestEventBus()
        {
            string eventName = "User.Upload.HeadImage";

            _eventBus.Subscribe(eventName, this);

            _eventBus.StartHandle(eventName);

            Thread.Sleep(2 * 1000);

            for (int i = 0; i < 100; ++i)
            {
                _eventBus.PublishAsync(eventName, $"Hello, Just say {i} times.");
            }

            Thread.Sleep(1 * 60 * 1000);

            _eventBus.UnSubscribe(eventName);
        }
    }
}
