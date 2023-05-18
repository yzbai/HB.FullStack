using System;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public class ServerHealthRes : SharedResource
    {
        public ServerHealthy ServerHealthy { get; set; }
        public override Guid? Id { get; set; }
        public override long? ExpiredAt { get; set; }
    }
}