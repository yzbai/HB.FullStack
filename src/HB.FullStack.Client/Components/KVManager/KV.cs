using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.KVManager
{
    public class KV : DbModel2<Guid>
    {
        public string Key { get; set; } = null!;

        public string? Value { get; set; }

        public DateTimeOffset ExpiredAt { get; set; }
        public override Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool Deleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string? LastUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}