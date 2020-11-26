#nullable enable

namespace HB.FullStack.Common.Entities
{
    public class UniqueGuidEntityPropertyAttribute : GuidEntityPropertyAttribute
    {
        public UniqueGuidEntityPropertyAttribute() : base()
        {
            Unique = true;
            NotNull = true;
        }
    }
}