#nullable enable

using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.Entities
{
    public class GuidEntityPropertyAttribute : EntityPropertyAttribute
    {
        public GuidEntityPropertyAttribute([CallerLineNumber] int number = 0) : base(number)
        {
            FixedLength = true;
            MaxLength = 32;
            //NotNull = true;
        }
    }
}