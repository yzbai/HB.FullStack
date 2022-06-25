using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public class ApiResourceDef
    {
        /// <summary>
        /// 在哪里取
        /// </summary>
        public string EndpointName { get; internal set; } = null!;

        /// <summary>
        /// 什么版本
        /// </summary>
        public string Version { get; internal set; } = null!;

        /// <summary>
        /// 用什么验证
        /// </summary>
        //public ApiAuthType AuthType { get; internal set; } = ApiAuthType.None;


        /// <summary>
        /// 取什么
        /// </summary>
        public string ResName { get; internal set; } = null!;

        public string? Parent1ResName { get; internal set; }

        public string? Parent2ResName { get; internal set; }

        public MethodInfo? Parent1ResIdGetMethod { get; internal set; }

        public MethodInfo? Parent2ResIdGetMethod { get; internal set; }


        //private string? _apiKeyName;
        //public string? ApiKeyName
        //{
        //    get => _apiKeyName;
        //    set
        //    {
        //        if (value.IsNullOrEmpty())
        //        {
        //            return;
        //        }

        //        _apiKeyName = value;

        //        AuthType = ApiAuthType.ApiKey;
        //    }
        //}


    }
}
