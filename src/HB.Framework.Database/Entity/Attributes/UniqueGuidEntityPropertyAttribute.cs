namespace HB.Framework.Database.Entity
{
    public class UniqueGuidEntityPropertyAttribute : GuidEntityPropertyAttribute
    {
        public UniqueGuidEntityPropertyAttribute() : base()
        {
            Unique = true;
        }
    }
}
