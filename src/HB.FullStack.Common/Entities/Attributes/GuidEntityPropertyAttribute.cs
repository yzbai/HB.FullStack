#nullable enable

namespace HB.FullStack.Common.Entities
{
    public class GuidEntityPropertyAttribute : EntityPropertyAttribute
    {
        public GuidEntityPropertyAttribute()
        {
            FixedLength = true;
            Length = 32;
            //NotNull = true;
        }
    }
}