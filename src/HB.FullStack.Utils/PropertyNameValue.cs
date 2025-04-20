namespace System
{
    public class PropertyNameValue
    {
        public string Name { get; set; } = null!;

        public object? Value { get; set; }

        public PropertyNameValue(string propertyName, object? value)
        {
            Name = propertyName;
            Value = value;
        }
    }
}
