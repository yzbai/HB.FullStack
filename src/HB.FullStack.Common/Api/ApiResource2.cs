using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiResource2 : ValidatableObject
    {
        [Range(0, int.MaxValue)]
        public int Version { get; set; } = -1;

        public string LastUser { get; set; } = string.Empty;

        public DateTimeOffset LastTime { get; set; }

        public abstract override int GetHashCode();
    }

    public abstract class GuidResource : ApiResource2
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        public sealed override int GetHashCode()
        {
            return HashCode.Combine(GetChildHashCode(), Id, LastTime, Version, LastUser);
        }

        protected abstract int GetChildHashCode();
    }

    public abstract class LongIdResource : ApiResource2
    {
        [LongId2]
        public long Id { get; set; } = -1;

        public sealed override int GetHashCode()
        {
            return HashCode.Combine(GetChildHashCode(), Id, LastTime, Version, LastUser);
        }

        protected abstract int GetChildHashCode();
    }
}
