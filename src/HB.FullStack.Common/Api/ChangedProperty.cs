namespace HB.FullStack.Common.Api
{
    public class ChangedProperty
    {
        public string PropertyName { get; set; } = null!;
        public string? PropertyOldStringValue { get; set; } = null!;
        public string? PropertyNewStringValue { get; set; } = null!;
    }
}