using System;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.Sync
{
    /// <summary>
    /// 使用自增保证顺序
    /// </summary>
    [PropertyTrackableObject]
    public partial class OfflineChange : DbModel2<long>
    {
        public OfflineChangeType Type { get; set; }

        public OfflineChangeStatus Status { get; set; }

        public Guid ModelId { get; set; }

        public string ModelFullName { get; set; } = null!;

        //public string? BusinessCatalog { get; set; }

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public PropertyChangePack? ChangePack { get; set; }

        //public string? DeletedObjectJson { get; set; }

        public long LastTime { get; set; } = TimeUtil.Timestamp;

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }

        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    public enum OfflineChangeType
    {
        Add = 0, //根据ModelId 和 ModelFullName去数据库取
        Update = 1,//PropertyChangePack
        UpdateProperties = 2,
        Delete = 3,//根据DeletedObjectJson
    }

    public enum OfflineChangeStatus
    {
        Waiting = 0,
        Failed = 1,
        Success = 2,
        Discard = 3
    }
}