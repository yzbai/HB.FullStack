namespace System
{
    public class NameValuePair
    {
        public string Name { get; set; } = null!;

        public object? Value { get; set; }

        public NameValuePair(string propertyName, object? value)
        {
            Name = propertyName;
            Value = value;
        }
    }
}
