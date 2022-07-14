using HB.FullStack.Database.DatabaseModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class OfflineHistory : TimestampAutoIncrementIdDBModel
    {
        public string ModelId { get; set; } = null!;

        public string ModelFullName { get; set; } = null!;

        public DbOperation Operation { get; set; }

        public DateTimeOffset OperationTime { get; set; }

        public bool Handled { get; set; }

    }
}
