﻿namespace HB.FullStack.Client.Components.Sync
{
    public enum SyncStatus
    {
        NotSynced_OfflineData,
        Syncing,
        SynceFailed,//失败, 有网络，但Server端不接受，要求特殊处理，即用户选择保存网络端还是本地 
        Synced //成功
    }
}