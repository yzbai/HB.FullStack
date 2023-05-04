using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Abstractions
{
    //TODO: 这里只考虑网络本身的状态，没有考虑服务器是否在线的问题

    /// <summary>
    /// Common Client Events
    /// 1. Device`s Events: Network, Battery, etc.
    /// 2. Application Events: App Start, App Resume, App Pause, etc.
    /// 3. UI Events: Page Appearing, Page Disappearing, etc.
    /// </summary>
    public interface IClientEvents
    {
        void Initialize();

        #region  ServerConnection
        
        bool ServerConnected { get; }

        //App启动，也会调用
        event Func<Task>? ServerConnectResumed;

        event Func<Task>? ServerConnectFailed;

        void ReportServerConnectFailed();

        #endregion

        #region Application

        event Func<Task>? AppStart;

        event Func<Task>? AppResume;

        event Func<Task>? AppSleep;

        event Func<Task>? AppExit;

        #endregion

    }
}
