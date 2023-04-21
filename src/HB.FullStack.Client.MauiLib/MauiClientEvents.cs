using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;
using HB.FullStack.Client.Abstractions;
using HB.FullStack.Common;

using Microsoft.Maui.Networking;

using static Microsoft.Maui.ApplicationModel.Permissions;

namespace HB.FullStack.Client.MauiLib
{
    //TODO: 测试，快速断网，然后连接，重复. 网络抖动

    public class MauiClientEvents : IClientEvents
    {
        private readonly WeakAsyncEventManager _eventManager = new WeakAsyncEventManager();

        public void Initialize()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            OnNetworkChanged(Connectivity.Current.NetworkAccess);
        }

        public void Close()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        #region Network

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

        public async Task OnNetworkResumed()
        {
            await _eventManager.RaiseEventAsync(nameof(NetworkResumed));
        }

        public async Task OnNetworkFailed()
        {
            await _eventManager.RaiseEventAsync(nameof(NetworkFailed));
        }

        #endregion

#pragma warning disable CS0067 // The event 'MauiClientEvents.AppStart' is never used
        public event Func<Task>? AppStart;
        public event Func<Task>? AppResume;
        public event Func<Task>? AppSleep;
        public event Func<Task>? AppExit;
#pragma warning restore CS0067 // The event 'MauiClientEvents.AppStart' is never used
    }
}
