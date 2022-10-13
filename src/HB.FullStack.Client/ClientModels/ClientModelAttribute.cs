namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ClientModelAttribute : Attribute
    {
        public static readonly int DefaultExpirySeconds = 3600;

        /// <summary>
        /// 几分钟就需要更新一次
        /// 从lasttime算起，这么长时间内是不需要请求api的。
        /// //TODO: 加上 文件配置，可以覆盖属性配置
        /// </summary>
        public int ExpirySeconds { get; }

        public bool AllowOfflineAdd { get; }
        public bool AllowOfflineUpdate { get; }
        public bool AllowOfflineDelete { get; }

        public bool AllowOfflineRead { get; } = false;

        //public bool NeedLogined { get; } = true;

        public ClientModelAttribute() : this(DefaultExpirySeconds, false, false, false, false) { }

        public ClientModelAttribute(int expirySeconds, bool allowOfflineRead, bool allowOfflineAdd, bool allowOfflineUpdate, bool allowOfflineDelete)
        {
            ExpirySeconds = expirySeconds;

            //NeedLogined = needLogined;
            AllowOfflineRead = allowOfflineRead;
            AllowOfflineUpdate = allowOfflineUpdate;
            AllowOfflineDelete = allowOfflineDelete;
            AllowOfflineAdd = allowOfflineAdd;
        }
    }
}
/*
//NOTICE: 几个过期时间
1. ClientModels 过期时间， 放在ClientModelDef里
2. StsToken 过期时间， 存储在StsToken里
3，DirectoryDescription过期时间，由客户端自己决定
*/