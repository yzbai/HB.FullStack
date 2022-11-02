using HB.FullStack.Common.Convert;

namespace System
{
    public static class StringConvertExtensions
    {
        public static string? ToString(this object? obj, StringConvertPurpose convertPurpose)
        {
            return StringConvertCenter.ConvertToString(obj, null, convertPurpose);
        }
    }
}
