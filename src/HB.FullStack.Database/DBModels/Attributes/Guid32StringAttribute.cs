

using System;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.DBModels
{
    public sealed class Guid32StringAttribute : DatabaseModelPropertyAttribute
    {
        public Guid32StringAttribute([CallerLineNumber] int propertyOrder = 0) : base(propertyOrder)
        {
            FixedLength = true;
            MaxLength = 32;
            //NotNull = true;
        }
    }
}