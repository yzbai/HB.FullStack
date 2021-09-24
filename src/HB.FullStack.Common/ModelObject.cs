using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Common
{
    public abstract class ApiResource2 : ValidatableObject
    {
        public int Version { get; set; } = -1;
    }

    public abstract class GuidResource : ApiResource2
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        
    }

    public abstract class LongIdResource : ApiResource2
    {
        [LongId2]
        public long Id { get; set; } = -1;

    }
}
