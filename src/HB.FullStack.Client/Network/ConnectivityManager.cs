using AsyncAwaitBestPractices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Network
{
    public abstract class ConnectivityManager : IDisposable
    {
        private readonly WeakEventManager _weakEventManager = new WeakEventManager();

        public event EventHandler OfflineDataReaded
        {
            add => _weakEventManager.AddEventHandler(value, nameof(OfflineDataReaded));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(OfflineDataReaded));
        }

        public void OnOfflineDataReaded()
        {
            _weakEventManager.RaiseEvent(nameof(OfflineDataReaded));
        }

        public ConnectivityStatus Status { get; protected set; }



        //TODO: 指示正在网络重连后的数据同步中，所以，其他的网络操作应该等一等
        //TODO: 应该存储到数据库中
        public bool NeedSyncAfterReconnected { get; internal set; }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ConnectivityManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
