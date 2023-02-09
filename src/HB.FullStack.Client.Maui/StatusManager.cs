using HB.FullStack.Client.Offline;
using HB.FullStack.Common;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui
{
    public class StatusManager : IStatusManager
    {
        private readonly WeakEventManager _eventManager = new WeakEventManager();

        public StatusManager()
        {
            OnceNetworkReady += async (sender, e) => { await StartSync(); };
        }

        #region Lifecycle

        //App创建
        public void OnAppConstructed()
        {

        }

        //进入方式1：App新建
        public void OnAppStart()
        {
            StartNetworkMonitor();
            StartUserMonitor();
        }

        //进入方式2：App恢复
        public void OnAppResume()
        {
            StartNetworkMonitor();
            StartUserMonitor();
        }

        //退出
        public void OnAppSleep()
        {
            StopUserMonitor();
            StopNetworkMonitor();
        }

        #endregion

        #region Network

        //TODO: 测试，快速断网，然后连接，重复. 网络抖动

        public NetworkStatus NetworkStatus { get; private set; }

        public event EventHandler OnceNetworkReady
        {
            add => _eventManager.AddEventHandler(value, nameof(OnceNetworkReady));
            remove => _eventManager.RemoveEventHandler(value, nameof(OnceNetworkReady));
        }

        public event EventHandler OnceNetworkFailed
        {
            add => _eventManager.AddEventHandler(value, nameof(OnceNetworkFailed));
            remove => _eventManager.RemoveEventHandler(value, nameof(OnceNetworkFailed));
        }

        public void EnsureNetworkReady()
        {
            if (NetworkStatus == NetworkStatus.Disconnected)
            {
                throw ClientExceptions.NoInternet();
            }
        }

        /// <summary>
        /// 如果Connectivity检测不准，实际无法使用Internet
        /// //TODO: 在ApiClient中捕获无网络异常
        /// </summary>
        public void ReportNoInternet()
        {
            if (NetworkStatus == NetworkStatus.Connected)
            {
                NetworkStatus = NetworkStatus.Disconnected;
                OnNetworkFailed();
            }
        }

        private void StartNetworkMonitor()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            NetworkStatus = GetNetworkStatus(Connectivity.Current.NetworkAccess);

            if (NetworkStatus == NetworkStatus.Connected)
            {
                OnNetworkReady();
            }
            else if (NetworkStatus == NetworkStatus.Disconnected)
            {
                OnNetworkFailed();
            }
        }

        private void StopNetworkMonitor()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            NetworkStatus now = GetNetworkStatus(e.NetworkAccess);

            if (now == this.NetworkStatus)
            {
                return;
            }

            NetworkStatus = now;

            if (NetworkStatus == NetworkStatus.Connected)
            {
                OnNetworkReady();
            }
            else if (NetworkStatus == NetworkStatus.Disconnected)
            {
                OnNetworkFailed();
            }
        }

        private void OnNetworkFailed()
        {
            _eventManager.HandleEvent(this, new EventArgs(), nameof(OnceNetworkFailed));
        }

        private void OnNetworkReady()
        {
            _eventManager.HandleEvent(this, new EventArgs(), nameof(OnceNetworkReady));
        }

        private static NetworkStatus GetNetworkStatus(NetworkAccess networkAccess)
        {
            return networkAccess switch
            {
                NetworkAccess.Unknown => NetworkStatus.Disconnected,
                NetworkAccess.None => NetworkStatus.Disconnected,
                NetworkAccess.Local => NetworkStatus.Disconnected,
                NetworkAccess.ConstrainedInternet => NetworkStatus.Disconnected,
                NetworkAccess.Internet => NetworkStatus.Connected,
                _ => NetworkStatus.Disconnected
            };
        }

        #endregion

        #region Syncing

        private readonly EventWaitHandle _syncedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, "Sync");

        public SyncStatus SyncStatus { get; private set; }

        public event Func<Task>? Syncing;

        public void WaitUntilSynced()
        {
            _syncedWaitHandle.WaitOne();

            EnsureSynced();
        }

        public void EnsureSynced()
        {
            if (SyncStatus != SyncStatus.Synced)
            {
                throw ClientExceptions.SyncError(SyncStatus);
            }
        }

        private async Task StartSync()
        {
            //1. ping check usertoken


            SyncStatus = SyncStatus.Syncing;

            try
            {
                _syncedWaitHandle.Reset();

                await OnSyncingAsync();

                SyncStatus = SyncStatus.Synced;
            }
            catch//(Exception ex)
            {
                SyncStatus = SyncStatus.SynceFailed;

                //TODO: 处理Sync Failed
                //1. 服务器无法连接,假网络,或者服务器挂了
                //2. 无法refresh token, 要求登录后再操作
                Currents.ShowToast("处理Sync Failed");

                //TODO: 处理完，set Status为Synced
            }
            finally
            {
                _syncedWaitHandle.Set();
            }
        }

        private Task OnSyncingAsync()
        {
            return Syncing?.Invoke() ?? Task.CompletedTask;
        }

        #endregion

        #region User

        private void StartUserMonitor()
        {
            throw new NotImplementedException();
        }

        private void StopUserMonitor()
        {
            throw new NotImplementedException();
        }

        public UserStatus UserStatus => throw new NotImplementedException();


        public void ReportLogouted()
        {
            throw new NotImplementedException();
        }

        public void ReportLogined()
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS0067 // Never Used.
        public event Func<Task>? Logined;
        public event Func<Task>? Logouted;
#pragma warning disable CS0067

        #endregion
    }
}
