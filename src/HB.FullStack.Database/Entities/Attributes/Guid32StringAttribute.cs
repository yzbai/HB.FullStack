#nullable enable

using System;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.Entities
{
    public class Guid32StringAttribute : EntityPropertyAttribute
    {
        public Guid32StringAttribute([CallerLineNumber] int number = 0) : base(number)
        {
            FixedLength = true;
            MaxLength = 32;
            //NotNull = true;
        }
    }
}