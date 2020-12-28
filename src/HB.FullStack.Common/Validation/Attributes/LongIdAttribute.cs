namespace System.ComponentModel.DataAnnotations
{
    public class LongIdAttribute : RangeAttribute
    {
        public LongIdAttribute() : base(0.0, long.MaxValue)
        {

        }

    }
}
