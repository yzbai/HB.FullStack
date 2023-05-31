using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public class ServerHealthRes : ValidatableObject, ISharedResource
    {
        public ServerHealthy ServerHealthy { get; set; }

        public long? ExpiredAt { get; set; }

        public ModelKind GetKind() => ModelKind.Shared;

    }
}