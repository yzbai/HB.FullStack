using System;
using System.Text.Json;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.Sync
{
    /// <summary>
    /// 使用自增保证顺序
    /// </summary>
    public class OfflineChange : DbModel<long>, ITimestamp
    {
        public OfflineChangeType Type { get; set; }

        public OfflineChangeStatus Status { get; set; }

        public string ModelIdString { get; set; } = null!;

        public string ModelFullName { get; set; } = null!;

        //public string? BusinessCatalog { get; set; }

        //TODO: 字段长度问题, 所有DbModel的字段长度检查
        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public PropertyChangePack? ChangePack { get; set; }

        //public string? DeletedObjectJson { get; set; }


        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }

        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
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