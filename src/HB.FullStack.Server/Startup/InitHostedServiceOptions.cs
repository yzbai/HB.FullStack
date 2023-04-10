using System;
using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Database;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.Startup
{
    public class InitHostedServiceOptions : IOptions<InitHostedServiceOptions>
    {
        public IEnumerable<DbInitContext> DbInitContexts { get; set; } = new List<DbInitContext>();

        public int DbInitLockWaitSeconds { get; set; } = 1 * 60;   

        public int DbInitLockExpireSeconds { get; set; } = 5 * 60;

        public InitHostedServiceOptions Value => this;
    }
}