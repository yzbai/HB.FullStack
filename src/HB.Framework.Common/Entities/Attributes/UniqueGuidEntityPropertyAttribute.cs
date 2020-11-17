#nullable enable

namespace HB.Framework.Common.Entities
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