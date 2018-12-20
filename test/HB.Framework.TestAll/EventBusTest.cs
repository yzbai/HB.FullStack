using System;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace HB.Framework.TestAll
{
    public class EventBusTest : IClassFixture<ServiceFixture>
    {
        private ITestOutputHelper _output;
        private IEventBus _eventBus;
        private ILogger _logger;

        public EventBusTest(ITestOutputHelper outputHelper, ServiceFixture service)
        {
            _output = outputHelper;
            _eventBus = service.EventBus;
            _logger = service.LoggerFactory.CreateLogger<EventBusTest>();
        }

        [Fact]
        public async Task PublistAndSubscribeTestAsync()
        {

            _logger.LogError("Test Message From Test");

            bool publishResult = true;

            for (int i = 0; i < 100; i++)
            {
                publishResult = await _eventBus.PublishAsync(new EventMessage("test.test1", "Hello, I publish this."));

                if (!publishResult)
                {
                    break;
                }
            }

            Assert.True(publishResult);

            _eventBus.Subscribe("test.test1", new EventHandler(_output));

            Thread.Sleep(10 * 1000);
        }
    }

    public class EventHandler : IEventHandler
    {
        public string Id { get; } = SecurityHelper.CreateUniqueToken();
        public string EventType { get; } = "test.test1";
        public ITestOutputHelper _output;

        public EventHandler(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Handle(string jsonData)
        {
            _output.WriteLine(jsonData);
        }
    }
}
