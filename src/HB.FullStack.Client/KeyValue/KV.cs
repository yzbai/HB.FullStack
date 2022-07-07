using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.Client.KeyValue
{
    public class KV : GuidDatabaseModel
    {
        public string Key { get; set; } = null!;

        public string? Value { get; set; }

        public DateTimeOffset ExpiredAt { get; set; }
    }
}