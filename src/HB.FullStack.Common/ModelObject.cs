using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Common
{
    public abstract class ModelObject2 : ValidatableObject
    {
        public int Version { get; set; } = -1;
    }

    public abstract class GuidModelObject : ModelObject2
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        
    }

    public abstract class LongIdModelObject : ModelObject2
    {
        [LongId2]
        public long Id { get; set; } = -1;

    }
}
