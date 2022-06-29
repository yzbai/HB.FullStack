using HB.FullStack.Common.Api;

using System;

namespace HB.FullStack.Common.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ApiResourceAttribute : Attribute
    {
        /// <summary>
        /// 资源在哪里
        /// </summary>
        public string EndPointName { get; }

        /// <summary>
        /// 什么版本
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 什么名字
        /// </summary>
        public string ResName { get; }

        /// <summary>
        /// 属于谁的子资源
        /// </summary>
        public string? Parent1ResName { get; set; }

        public string? Parent2ResName { get; set; }

        public ApiResourceAttribute(string endPointName, string version, string resName)
        {
            EndPointName = endPointName;
            Version = version;
            ResName = resName;
        }
    }
}
