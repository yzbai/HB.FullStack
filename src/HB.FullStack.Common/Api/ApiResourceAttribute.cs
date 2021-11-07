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

        public string ResName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPointName"></param>
        /// <param name="version"></param>
        /// <param name="rateLimitMilliseconds"></param>
        /// <param name="resName">一般是复数形式</param>
        public ApiResourceAttribute(string endPointName, string version, int rateLimitMilliseconds, string resName)
        {
            EndPointName = endPointName;
            Version = version;
            RateLimitMilliseconds = rateLimitMilliseconds;
            ResName = resName;
        }


    }
}
