#nullable enable

using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.Entities
{
    public class GuidStringAttribute : EntityPropertyAttribute
    {
        public GuidStringAttribute([CallerLineNumber] int number = 0) : base(number)
        {
            FixedLength = true;
            MaxLength = 32;
            //NotNull = true;
        }
    }
}