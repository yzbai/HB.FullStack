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
        /// 验证方式
        /// </summary>
        public ApiAuthType AuthType { get; private set; }

        /// <summary>
        /// 什么名字
        /// </summary>
        public string ResName { get; }

        /// <summary>
        /// 属于谁的子资源
        /// </summary>
        public string? Parent1ResName { get; set; }

        private string? _apiKeyName;

        public string? ApiKeyName
        {
            get => _apiKeyName;
            set
            {
                if(value.IsNullOrEmpty())
                {
                    return;
                }

                _apiKeyName = value;

                AuthType = ApiAuthType.ApiKey;
            }
        }


        public ApiResourceAttribute(string endPointName, string version, ApiAuthType authType, string resName)
        {
            EndPointName = endPointName;
            Version = version;
            AuthType = authType;
            ResName = resName;
        }
    }
}
