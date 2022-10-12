using System;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Offline
{
    /// <summary>
    /// 使用自增保证顺序
    /// </summary>
    public class OfflineChange : TimelessAutoIncrementIdDbModel
    {
        public OfflineChangeType Type { get; set; }

        public OfflineChangeStatus Status { get; set; }

        public Guid ModelId { get; set; }

        public string ModelFullName { get; set; } = null!;

        //public string? BusinessCatalog { get; set; }

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public ChangedPack? ChangedPack { get; set; }

        public string? DeletedObjectJson { get; set; }

        public long LastTime { get; set; } = TimeUtil.Timestamp;
    }

    public enum OfflineChangeType
    {
        Add = 0, //根据ModelId 和 ModelFullName去数据库取
        Update = 1,//ChangedPack
        Delete = 2,//根据DeletedObjectJson
    }

    public enum OfflineChangeStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2
    }

}
