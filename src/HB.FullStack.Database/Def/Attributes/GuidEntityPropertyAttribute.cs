#nullable enable

namespace HB.FullStack.Database.Def
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