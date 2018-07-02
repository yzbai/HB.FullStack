using HB.Framework.Common;
using HB.Framework.Database;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using HB.Framework.KVStore;
using HB.PresentFish.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CommonSample
{
    public class TextWrapper
    {
        public string Text { get; set; }
    }

    public class TestEventHandler : IEventHandler, IDisposable
    {
        public TestEventHandler() { }

        public EventHandlerConfig GetConfig()
        {
            throw new NotImplementedException();
        }

        public void Handle(string jsonString)
        {
            throw new NotImplementedException();
        }

        public void handleConsumerEvent(string jsonData)
        {
            Console.WriteLine("Customer.Event.Text Handled: " + jsonData);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~TestEventHandler()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class TestEventHandler2 : IEventHandler, IDisposable
    {
        public TestEventHandler2() { }

 

        public EventHandlerConfig GetConfig()
        {
            throw new NotImplementedException();
        }

        public void Handle(string jsonString)
        {
            throw new NotImplementedException();
        }

        public void handleEvent(string jsonData)
        {
            Console.WriteLine("KafkaTest.Event.Text " + jsonData);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~TestEventHandler2()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    class KafkaTestBiz
    {
        private IApplicationLifetime _applife;
        private IEventBus _eventBus;

        public KafkaTestBiz(IApplicationLifetime applife, IDatabase database, IKVStore kvstore, IDistributedCache cache, IEventBus eventBus, ILogger<KafkaTestBiz> logger)
        {
            _applife = applife;
            _eventBus = eventBus;

            _eventBus.Handle();
        }



        private async Task DoBizAsync(string eventName, string text)
        {
            TextWrapper tx = new TextWrapper() { Text = text };

            await _eventBus.Publish(eventName, DataConverter.ToJson(tx));
        }

        internal static async Task EventBusTestAsync()
        {
            KafkaTestBiz biz = ActivatorUtilities.CreateInstance<KafkaTestBiz>(Program.Services);

            string text;
            int i = 0;

            while ((text = Console.ReadLine()) != "q")
            {
                if (i++ % 2 == 0)
                {
                    await biz.DoBizAsync("KafkaTest.Event.Text", text);
                }
                else
                {
                    await biz.DoBizAsync("Cusomer.Event.Text", text);
                }
            }

            biz.Close();
        }

        private void Close()
        {
            _applife.StopApplication();
        }
    }



}
