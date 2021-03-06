namespace System.ComponentModel.DataAnnotations
{
    public sealed class LongIdAttribute : RangeAttribute
    {
        public LongIdAttribute() : base(1.0, long.MaxValue)
        {

        }

    }
}
