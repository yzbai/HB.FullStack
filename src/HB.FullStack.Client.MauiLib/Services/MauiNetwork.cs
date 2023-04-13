using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Client.Services;
using HB.FullStack.Common;

using Microsoft.Maui.Networking;

using static Microsoft.Maui.ApplicationModel.Permissions;

namespace HB.FullStack.Client.MauiLib.Services
{
    //TODO: 测试，快速断网，然后连接，重复. 网络抖动

    public class MauiNetwork : INetwork
    {
        private readonly WeakAsyncEventManager _eventManager = new WeakAsyncEventManager();

        public bool NetworkIsReady => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        public event Func<Task>? NetworkResumed
        {
            add => _eventManager.Add(value, nameof(NetworkResumed));
            remove => _eventManager.Remove(value, nameof(NetworkResumed));
        }

        public event Func<Task>? NetworkFailed
        {
            add => _eventManager.Add(value, nameof(NetworkFailed));
            remove => _eventManager.Remove(value, nameof(NetworkFailed));
        }

        public void Initialize()
        {
            StartNetworkMonitor();

            OnNetworkChanged(Connectivity.Current.NetworkAccess);
        }

        private void StartNetworkMonitor()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        private void StopNetworkMonitor()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            OnNetworkChanged(e.NetworkAccess);
        }

        private void OnNetworkChanged(NetworkAccess networkAccess)
        {
            if (networkAccess == NetworkAccess.Internet)
            {
                OnNetworkResumed().SafeFireAndForget(ex =>
                {
                    //TODO: Deal with this
                });
            }
            else
            {
                OnNetworkFailed().SafeFireAndForget(ex =>
                {
                    //TODO: Deal with this
                });
            }
        }


        private async Task OnNetworkResumed()
        {
            await _eventManager.RaiseEventAsync(nameof(NetworkResumed));
        }

        private async Task OnNetworkFailed()
        {
            await _eventManager.RaiseEventAsync(nameof(NetworkFailed));
        }
    }
}
