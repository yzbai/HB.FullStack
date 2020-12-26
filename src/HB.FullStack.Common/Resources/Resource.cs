using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Common.Resources
{
    public abstract class Resource : ValidatableObject
    {
        [IdBarrier]
        public long Id { get; set; } = -1;
    }
}
