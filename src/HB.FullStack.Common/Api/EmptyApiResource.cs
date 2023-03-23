namespace HB.FullStack.Common.Api
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
