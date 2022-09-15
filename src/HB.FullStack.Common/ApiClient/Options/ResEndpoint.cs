using System;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    /// <summary>
    /// Res对应的ModelController或者PlainUrl.
    /// 一个资源对应一个ModelController，而一个ModelController可以对应多个资源
    /// ResEndpoint来源：1，通过手写配置在AddService中；2，通过ResEndpointAttribute中指定；3，默认
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
        public string ControllerOrPlainUrl { get; set; } = null!;

        /// <summary>
        /// 默认的GetRequest的Auth
        /// 默认权限是指没有指定Condition下的权限.
        /// 可以在ApiRequest中覆盖
        /// </summary>
        public ApiRequestAuth DefaultReadAuth { get; set; } = null!;

        /// <summary>
        /// 默认的Put,UpdateFields,Add,Delete权限
        /// 默认权限是指没有指定Condition下的权限.
        /// 可以在ApiRequest中覆盖
        /// </summary>
        public ApiRequestAuth DefaultWriteAuth { get; set; } = null!;

        public SiteSetting? SiteSetting { get; set; }

        public ResEndpoint(string resName)
        {
            Type = ResEndpointType.ControllerModel;
            ResName = resName;
            ControllerOrPlainUrl = resName.RemoveSuffix("Res");
            DefaultReadAuth = ApiRequestAuth.JWT;
            DefaultWriteAuth = ApiRequestAuth.JWT;
        }

        public ResEndpoint(ResEndpointType type, string resName, string controllerOrPlainUrl, ApiRequestAuth defaultReadAuth, ApiRequestAuth defaultWriteAuth)
        {
            Type = type;
            ResName = resName;
            ControllerOrPlainUrl = controllerOrPlainUrl;
            DefaultReadAuth = defaultReadAuth;
            DefaultWriteAuth = defaultWriteAuth;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ResName, ControllerOrPlainUrl);
        }
    }
}
