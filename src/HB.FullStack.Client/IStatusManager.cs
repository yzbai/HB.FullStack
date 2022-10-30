using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

namespace HB.FullStack.Client
{
    public interface IStatusManager
    {
        #region Network

        bool IsNetworkDown() => NetworkStatus != NetworkStatus.Connected;

        NetworkStatus NetworkStatus { get; }

        event EventHandler OnceNetworkReady;
        
        event EventHandler OnceNetworkFailed;

        void EnsureNetworkReady();
        
        void ReportNoInternet();

        #endregion

        #region Syncing
        
        SyncStatus SyncStatus { get; }

        event Func<Task>? Syncing;

        void WaitUntilSynced();

        void EnsureSynced();

        #endregion

        #region User

        UserStatus UserStatus { get; }

        event Func<Task>? Logined;
        event Func<Task>? Logouted;

        void ReportLogouted();

        void ReportLogined();

        #endregion
    }

    public enum NetworkStatus
    {
        Disconnected,

        Connected
    }

    public enum SyncStatus
    {
        Syncing,
        SynceFailed,//失败 
        Synced //成功
    }

    public enum UserStatus
    {
        UnLogined,
        Logined
    }
}
