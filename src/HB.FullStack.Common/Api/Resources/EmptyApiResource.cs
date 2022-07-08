namespace HB.FullStack.Common.Api.Resources
{
    public class EmptyApiResource : DTO
    {
        public static EmptyApiResource Value { get; }

        static EmptyApiResource()
        {
            Value = new EmptyApiResource();
        }

        private EmptyApiResource() { }
    }
}