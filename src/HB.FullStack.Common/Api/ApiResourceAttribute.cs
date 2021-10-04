using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ApiResourceAttribute : Attribute
    {
        public string EndPointName { get; }

        public string Version { get; }
        
        /// <summary>
        /// 有效时间，即多久请求一次即可
        /// </summary>
        public TimeSpan RateLimit { get; }

        public ApiResourceAttribute(string endPointName, string version, int rateLimitMilliseconds)
        {
            EndPointName = endPointName;
            Version = version;
            RateLimit = TimeSpan.FromMilliseconds(rateLimitMilliseconds);
        }
    }
}
