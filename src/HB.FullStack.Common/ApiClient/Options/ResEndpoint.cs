using System;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    /// <summary>
    /// Res对应的ModelController或者PlainUrl.
    /// 一个资源对应一个ModelController，而一个ModelController可以对应多个资源
    /// </summary>
    public class ResEndpoint
    {
        public ResEndpointType Type { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResName { get; set; } = null!;

        /// <summary>
        /// 当Type为ControllerModel时，BindingValue为Controller名称
        /// 当Type为PlainUrl时，BindingValue为除去BaseUrl剩下的url
        /// </summary>
        public string ControllerOrUrl { get; set; } = null!;

        /// <summary>
        /// 默认的GetRequest的Auth
        /// 默认权限是指没有指定Condition下的权限.
        /// 可以在ApiRequest中覆盖
        /// </summary>
        public ApiRequestAuth2 DefaultReadAuth { get; set; } = null!;

        /// <summary>
        /// 默认的Put,UpdateFields,Add,Delete权限
        /// 默认权限是指没有指定Condition下的权限.
        /// 可以在ApiRequest中覆盖
        /// </summary>
        public ApiRequestAuth2 DefaultWriteAuth { get; set; } = null!;

        public SiteSetting? SiteSetting { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ResName, ControllerOrUrl);
        }
    }
}
