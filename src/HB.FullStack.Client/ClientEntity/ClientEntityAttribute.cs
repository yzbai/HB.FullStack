using System;

namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ClientEntityAttribute : Attribute
    {
        public static readonly int DefaultExpirySeconds = 3600;

        /// <summary>
        /// 几分钟就需要更新一次
        /// 从lasttime算起，这么长时间内是不需要请求api的。
        /// //TODO: 加上 文件配置，可以覆盖属性配置
        /// </summary>
        public int ExpirySeconds { get; }

        public bool AllowOfflineWrite { get; }

        public bool AllowOfflineRead { get; } = true;

        public bool NeedLogined { get; } = true;

        public ClientEntityAttribute() : this(DefaultExpirySeconds, true, true, false) { }

        public ClientEntityAttribute(int expirySeconds, bool needLogined, bool allowOfflineRead, bool allowOfflineWrite)
        {
            ExpirySeconds = expirySeconds;

            NeedLogined = needLogined;
            AllowOfflineRead = allowOfflineRead;
            AllowOfflineWrite = allowOfflineWrite;
        }
    }
}
/*
//NOTICE: 几个过期时间
1. ClientEntity 过期时间， 放在ClientEntityDef里
2. StsToken 过期时间， 存储在StsToken里
3，DirectoryDescription过期时间，由客户端自己决定
*/