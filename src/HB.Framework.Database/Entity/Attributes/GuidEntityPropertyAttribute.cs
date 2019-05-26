namespace HB.Framework.Database.Entity
{
    public class GuidEntityPropertyAttribute : EntityPropertyAttribute
    {
        public GuidEntityPropertyAttribute()
        {
            FixedLength = true;
            Length = 36;
            NotNull = true;
        }
    }
}
