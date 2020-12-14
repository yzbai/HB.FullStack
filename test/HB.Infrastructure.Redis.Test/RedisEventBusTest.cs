using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack;
using HB.FullStack.EventBus.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace HB.Infrastructure.Redis.Test
{
    public class RedisEventBusTest : IClassFixture<ServiceFixture_MySql>, IEventHandler
    {
        private readonly ITestOutputHelper _output;
        private readonly IEventBus _eventBus;

        public RedisEventBusTest(ITestOutputHelper testOutputHelper, ServiceFixture_MySql serviceFixture)
        {
            _output = testOutputHelper;
            _eventBus = serviceFixture.ServiceProvider.GetRequiredService<IEventBus>();
        }

        [Fact]
        public async Task TestEventBusAsync()
        {
            string eventName = "User.Upload.HeadImage";

            _eventBus.Subscribe(eventName, this);

            _eventBus.StartHandle(eventName);

            Thread.Sleep(2 * 1000);

            for (int i = 0; i < 100; ++i)
            {
                await _eventBus.PublishAsync(eventName, $"Hello, Just say {i} times.").ConfigureAwait(false);
            }

            Thread.Sleep(1 * 60 * 1000);

            await _eventBus.UnSubscribeAsync(eventName).ConfigureAwait(false);
        }

        public Task HandleAsync(string jsonData, CancellationToken cancellationToken)
        {
            _output.WriteLine(jsonData);
            return Task.CompletedTask;
        }
    }
}
