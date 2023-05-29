using System;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public interface IServerHealthRes : ISharedResource
    {
        public ServerHealthy ServerHealthy { get; set; }

    }
}