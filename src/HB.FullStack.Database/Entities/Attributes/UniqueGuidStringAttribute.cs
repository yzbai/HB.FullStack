#nullable enable

using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.Entities
{
    public class UniqueGuidStringAttribute : GuidStringAttribute
    {
        public UniqueGuidStringAttribute([CallerLineNumber] int number = 0) : base(number)
        {
            Unique = true;
            NotNull = true;
        }
    }
}