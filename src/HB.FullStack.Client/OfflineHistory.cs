using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client
{
    public enum DbOperation
    {
        Add,
        Update,
        Delete,
    }

    /// <summary>
    /// 使用自增保证顺序
    /// </summary>
    public class OfflineHistory : TimestampAutoIncrementIdDbModel
    {
        public string ModelId { get; set; } = null!;

        public string ModelFullName { get; set; } = null!;

        public DbOperation Operation { get; set; }

        public DateTimeOffset OperationTime { get; set; }

        public bool Handled { get; set; }

    }
}
