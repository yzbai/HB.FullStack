using System;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Components.HealthCheck;
using HB.FullStack.Common;
using HB.FullStack.Common.Shared;

using Microsoft.Maui.Dispatching;
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

        private IDispatcherTimer? _serverConnectCheckTimer = null;

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
            if (_serverConnectCheckTimer != null)
            {
                _serverConnectCheckTimer.Stop();
                _serverConnectCheckTimer = null;
            }

            ServerConnected = true;
            await _eventManager.RaiseEventAsync(nameof(ServerConnectResumed));
        }

        private readonly object _lockObj = new object();

        private async Task OnServerConnectFailed()
        {
            ServerConnected = false;
            await _eventManager.RaiseEventAsync(nameof(ServerConnectFailed));

            //TODO: Start to monitor the network

            if (_serverConnectCheckTimer == null)
            {
                lock (_lockObj)
                {
                    if (_serverConnectCheckTimer == null)
                    {
                        _serverConnectCheckTimer = Currents.Page.Dispatcher.CreateTimer();

                        _serverConnectCheckTimer.Interval = TimeSpan.FromSeconds(15);
                        _serverConnectCheckTimer.IsRepeating = true;
                        _serverConnectCheckTimer.Tick += async (_, _) =>
                        {
                            bool connected = await TryConnectServerAsync();

                            if(connected)
                            {
                                await OnServerConnectResumed();
                            }
                        };

                        _serverConnectCheckTimer.Start();
                    }
                }
            }
        }

        private async void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            await OnNetworkChangedAsync();
        }

        private async Task<bool> TryConnectServerAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.None)
            {
                try
                {
                    ServerHealthRes? res = await _apiClient.GetAsync<ServerHealthRes>(new ServerHealthGetRequest());

                    if (res?.ServerHealthy == ServerHealthy.UP)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private async Task OnNetworkChangedAsync()
        {

            bool connected = await TryConnectServerAsync();

            if (connected)
            {
                await OnServerConnectResumed();
            }
            else
            {
                await OnServerConnectFailed();
            }
        }

        public async Task ReportServerConnectFailedAsync()
        {
            await OnServerConnectFailed();
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
