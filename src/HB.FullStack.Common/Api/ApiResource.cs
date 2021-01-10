using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiResource : ValidatableObject
    {
        [IdBarrier]
        public long Id { get; set; } = -1;
    }
}
