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
    public class OfflineChange : ClientDbModel
    {
        public OfflineChangeType Type { get; set; }

        public OfflineChangeStatus Status { get; set; }

        public Guid ModelId { get; set; }

        public string ModelFullName { get; set; } = null!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public ChangedPack? ChangedPack { get; set; }

    }
}
