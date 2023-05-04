using System;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Components.HealthCheck;
using HB.FullStack.Common;
using HB.FullStack.Common.Shared;

using IdentityLookup;

using Microsoft.Maui.Networking;

namespace HB.FullStack.Client.MauiLib
{
    //TODO: 测试，快速断网，然后连接，重复. 网络抖动

    public class MauiClientEvents : IClientEvents
    {
        private readonly WeakAsyncEventManager _eventManager = new WeakAsyncEventManager();
        private readonly IApiClient _apiClient;

        public MauiClientEvents(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public void Initialize()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            OnNetworkChangedAsync().SafeFireAndForget();
        }

        public void Close()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        #region SeverConnection

        public bool ServerConnected { get; set; }

        public event Func<Task>? ServerConnectResumed
        {
            add => _eventManager.Add(value, nameof(ServerConnectResumed));
            remove => _eventManager.Remove(value, nameof(ServerConnectResumed));
        }

        public event Func<Task>? ServerConnectFailed
        {
            add => _eventManager.Add(value, nameof(ServerConnectFailed));
            remove => _eventManager.Remove(value, nameof(ServerConnectFailed));
        }

        private async Task OnServerConnectResumed()
        {
            ServerConnected = true;
            await _eventManager.RaiseEventAsync(nameof(ServerConnectResumed));
        }

        private async Task OnServerConnectFailed()
        {
            ServerConnected = false;
            await _eventManager.RaiseEventAsync(nameof(ServerConnectFailed));

            //Start to monitor the network
             Currents.Dispatcher.CreateTimer();
        }

        private async void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            await OnNetworkChangedAsync();
        }

        private async Task OnNetworkChangedAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.None)
            {
                try
                {
                    ServerHealthRes? res = await _apiClient.GetAsync<ServerHealthRes>(new ServerHealthGetRequest());

                    if (res?.ServerHealthy == ServerHealthy.UP)
                    {
                        await OnServerConnectResumed();
                        return;
                    }
                }
                catch 
                {
                    await OnServerConnectFailed(); 
                    return;
                }
            }

            await OnServerConnectFailed();

            //TODO: 每隔15s尝试一次
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
