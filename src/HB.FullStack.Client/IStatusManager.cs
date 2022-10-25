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

        NetworkStatus NetworkStatus { get; }

        event EventHandler OnceNetworkReady;
        event EventHandler OnceNetworkFailed;

        void EnsureNetworkReady();
        void ReportNoInternet();

        #endregion

        #region Syncing

        event Func<Task>? Syncing;

        void WaitUntilSynced();

        void EnsureSynced();

        #endregion
    }
}
