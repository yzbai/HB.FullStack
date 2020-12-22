#nullable enable

namespace HB.FullStack.Database.Def
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