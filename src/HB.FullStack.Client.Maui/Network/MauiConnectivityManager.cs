using HB.FullStack.Client.Network;

using Microsoft.Maui.Networking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Network
{
    public class MauiConnectivityManager : ConnectivityManager
    {
        public MauiConnectivityManager()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            SetStatus(e.NetworkAccess);

            //TODO: 执行数据同步
        }

        private bool _isDiposed;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_isDiposed)
            {
                if (disposing)
                {
                    // managed
                }
                //unmanaged

                _isDiposed = true;
            }
        }

        private void SetStatus(NetworkAccess access)
        {
            if (access == NetworkAccess.Internet)
            {
                Status = NeedSyncAfterReconnected ? ConnectivityStatus.ConnectedSyncing : ConnectivityStatus.ConnectedReady;
            }
            Status = ConnectivityStatus.Disconnected;
        }
    }
}
