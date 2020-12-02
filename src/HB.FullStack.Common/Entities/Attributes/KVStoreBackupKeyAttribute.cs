using System;

namespace HB.FullStack.Common.Entities
{
    /// <summary>
    /// 当KVStoreEntity中没有显示使用KVStoreKeyAttribute标记key时，寻找此属性代表的BackupKey
    /// 存在KVStoreKey时，此BackupKey不起作用
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KVStoreBackupKeyAttribute : System.Attribute
    {
        public KVStoreBackupKeyAttribute()
        {
        }
    }
}