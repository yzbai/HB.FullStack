using System;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    /// <summary>
    /// 本质是解决ApiResource的来源问题，与谁binding,哪个Model是这个Dto的根
    /// </summary>
    public class ResBinding
    {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResName { get; set; } = null!;

        public ResBindingType Type { get; set; }

        /// <summary>
        /// 当Type为ControllerModel时，BindingValue为Controller名称
        /// 当Type为PlainUrl时，BindingValue为除去BaseUrl剩下的url
        /// </summary>
        public string BindingValue { get; set; } = null!;

        /// <summary>
        /// 默认的GetRequest的Auth
        /// 默认权限是指没有指定Condition下的权限.
        /// 可以在ApiRequest中覆盖
        /// </summary>
        public ApiRequestAuth ReadAuth { get; set; }

        /// <summary>
        /// 默认的Put,Patch,Post,Delete权限
        /// 默认权限是指没有指定Condition下的权限.
        /// 可以在ApiRequest中覆盖
        /// </summary>
        public ApiRequestAuth WriteAuth { get; set; }

        public EndpointSetting? EndpointSetting { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ResName, BindingValue);
        }
    }
}
