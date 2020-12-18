#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

using HB.FullStack.Common.Entities.Attributes;

[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]

namespace HB.FullStack.Common.Entities
{
    /// <summary>
    /// 数据库表类型。 一个TableDomain对应一张数据库表。
    /// 所有数据库表类型必须继承此类。
    /// 内置支持IList 与 IDictionary 字段
    /// 配合DatabaseTypeConverter，可以存储任意自定义字段
    /// </summary>
    //[Serializable]
    public class Entity : ValidatableObject
    {
        public const int LastUserMaxLength = 100;

        [PropertyOrder]
        [AutoIncrementPrimaryKey]
        [EntityProperty]
        public long Id { get; internal set; } = -1;

        //TODO: 思考这里最好是internal set，但是由于dto转换，不能，思考解决办法
        /// <summary>
        /// 资源ID，全局不变
        /// </summary>
        [PropertyOrder]
        [Required]
        [UniqueGuidEntityProperty]
        [KVStoreBackupKey]
        [CacheGuidKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [PropertyOrder]
        [EntityProperty]
        public int Version { get; internal set; } = -1;

        [PropertyOrder]
        [EntityProperty]
        public string LastUser { get; internal set; } = string.Empty;

        /// <summary>
        /// UTC 时间
        /// </summary>
        [PropertyOrder]
        [EntityProperty]
        public DateTimeOffset LastTime { get; internal set; } = TimeUtil.UtcNow;

        [PropertyOrder]
        [EntityProperty]
        public bool Deleted { get; internal set; } = false;
    }
}