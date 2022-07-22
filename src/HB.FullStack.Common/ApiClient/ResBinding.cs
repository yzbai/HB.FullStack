using System;

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

        public EndpointSetting? EndpointSetting { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ResName, BindingValue);
        }
    }
}
