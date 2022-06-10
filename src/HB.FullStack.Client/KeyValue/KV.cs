using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.Entities;

namespace HB.FullStack.Client.KeyValue
{
    public class KV : GuidEntity
    {
        public string Key { get; set; } = null!;

        public string? Value { get; set; }

        public DateTimeOffset ExpiredAt { get; set; }
    }
}