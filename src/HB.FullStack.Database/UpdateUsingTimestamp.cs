using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;

namespace HB.FullStack.Database
{
    public class UpdateUsingTimestamp
    {
        public object? Id { get; set; }

        public long? OldTimestamp { get; set; }

        public long? NewTimestamp { get; set; }

        public IList<string> PropertyNames { get; set; } = new List<string>();

        public IList<object?> NewPropertyValues { get; set; } = new List<object?>();
        
    }
}
