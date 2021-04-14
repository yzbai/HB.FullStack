#nullable enable

using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.Entities
{
    public class UniqueGuidEntityPropertyAttribute : GuidEntityPropertyAttribute
    {
        public UniqueGuidEntityPropertyAttribute([CallerLineNumber] int number = 0) : base(number)
        {
            Unique = true;
            NotNull = true;
        }
    }
}