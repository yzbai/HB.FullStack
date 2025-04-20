namespace System
{
    public interface IStringConverter
    {
        Type ObjectType { get; }

        string? ConvertToString(object? obj, StringConvertPurpose purpose);

        object? ConvertFromString(string? str, StringConvertPurpose purpose);
    }
}
