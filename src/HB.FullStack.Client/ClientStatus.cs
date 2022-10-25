using System;

namespace HB.FullStack.Client
{
    //Network
    public enum NetworkStatus
    {
        Disconnected,
        
        Connected
    }

    public enum SyncStatus
    {
        Syncing,
        SynceFailed,
        Synced
    }

    public enum UserStatus
    {
        UnLogined,
        Logined
    }
}
