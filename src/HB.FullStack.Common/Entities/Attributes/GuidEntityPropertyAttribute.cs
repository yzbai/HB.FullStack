#nullable enable

namespace HB.FullStack.Common.Entities
{
    public class GuidEntityPropertyAttribute : EntityPropertyAttribute
    {
        public GuidEntityPropertyAttribute()
        {
            FixedLength = true;
            MaxLength = 32;
            //NotNull = true;
        }
    }
}