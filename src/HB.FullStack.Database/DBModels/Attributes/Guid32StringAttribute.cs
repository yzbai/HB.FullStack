

using System;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.DbModels
{
    public sealed class Guid32StringAttribute : DbModelPropertyAttribute
    {
        public Guid32StringAttribute([CallerLineNumber] int propertyOrder = 0) : base(propertyOrder)
        {
            FixedLength = true;
            MaxLength = 32;
            //NotNull = true;
        }
    }
}