

using System;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.DbModels
{
    public sealed class DbGuid32StringFieldAttribute : DbFieldAttribute
    {
        public DbGuid32StringFieldAttribute([CallerLineNumber] int propertyOrder = 0) : base(propertyOrder)
        {
            FixedLength = true;
            MaxLength = 32;
            //NotNull = true;
        }
    }
}