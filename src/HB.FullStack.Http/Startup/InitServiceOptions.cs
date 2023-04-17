using System;
using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Database;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.WebLib.Startup
{
    public class InitServiceOptions : IOptions<InitServiceOptions>
    {
        public IEnumerable<DbInitContext> DbInitContexts { get; set; } = new List<DbInitContext>();

        public int DbInitLockWaitSeconds { get; set; } = 1 * 60;   

        public int DbInitLockExpireSeconds { get; set; } = 5 * 60;

        public InitServiceOptions Value => this;
    }
}