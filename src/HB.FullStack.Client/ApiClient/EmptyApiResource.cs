using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    public class EmptyApiResource : SharedResource
    {
        public static EmptyApiResource Value { get; }

        static EmptyApiResource()
        {
            Value = new EmptyApiResource();
        }

        private EmptyApiResource() { }
    }
}
