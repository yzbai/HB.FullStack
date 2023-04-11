namespace HB.FullStack.Common.Shared
{
    public class EmptyApiResource : ApiResource
    {
        public static EmptyApiResource Value { get; }

        static EmptyApiResource()
        {
            Value = new EmptyApiResource();
        }

        private EmptyApiResource() { }
    }
}
