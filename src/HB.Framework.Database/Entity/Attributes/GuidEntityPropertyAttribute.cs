namespace HB.Framework.Database.Entity
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
