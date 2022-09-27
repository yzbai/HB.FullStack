using System;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Offline
{
    /// <summary>
    /// 使用自增保证顺序
    /// </summary>
    public class OfflineHistory : ClientDbModel
    {
        public Guid ModelId { get; set; }

        public string ModelFullName { get; set; } = null!;

        public HistoryType HistoryType { get; set; }

        public bool Handled { get; set; }

    }
}
