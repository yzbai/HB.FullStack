#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Framework.Common.Entities
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

        [AutoIncrementPrimaryKey]
        [EntityProperty("Id")]
        public long Id { get; set; } = -1;

        /// <summary>
        /// 资源ID，全局不变
        /// </summary>
        [Required]
        [UniqueGuidEntityProperty]
        [KVStoreBackupKey]
        [CacheGuidKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [EntityProperty("Version")]
        public int Version { get; set; } = 0;

        [EntityProperty("上一次更改者", Length = LastUserMaxLength)]
        public string LastUser { get; set; } = string.Empty;

        /// <summary>
        /// UTC 时间
        /// </summary>
        [EntityProperty("上一次更改时间")]
        public DateTimeOffset LastTime { get; set; } = DateTimeOffset.UtcNow;

        [EntityProperty("逻辑删除标志")]
        public bool Deleted { get; set; } = false;
    }
}