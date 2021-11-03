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
        public int RateLimitMilliseconds { get; }

        public string ResourceName { get; }

        public string ResourceCollectionName { get; }

        public ApiResourceAttribute(string endPointName, string version, int rateLimitMilliseconds, string resourceName, string resourceCollectionName)
        {
            EndPointName = endPointName;
            Version = version;
            RateLimitMilliseconds = rateLimitMilliseconds;
            ResourceName = resourceName;
            ResourceCollectionName = resourceCollectionName;
        }


    }
}
