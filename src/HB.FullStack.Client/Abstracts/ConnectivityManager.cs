using AsyncAwaitBestPractices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client
{
    public abstract class ConnectivityManager
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

        public abstract bool IsInternetConnected();
    }
}
