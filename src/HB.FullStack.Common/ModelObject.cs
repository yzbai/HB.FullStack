using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Common
{
    public abstract class ModelObject : ValidatableObject
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        public int Version { get; set; } = -1;

    }
}
