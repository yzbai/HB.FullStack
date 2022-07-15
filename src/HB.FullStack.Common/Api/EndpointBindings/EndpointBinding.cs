namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 本质是解决ApiResource的来源问题，与谁binding,哪个Model是这个Dto的根
    /// </summary>
    public class EndpointBinding
    {
        /// <summary>
        /// 站点
        /// </summary>
        public string EndpointName { get; internal set; } = null!;

        /// <summary>
        /// 什么版本
        /// </summary>
        public string Version { get; internal set; } = null!;


        /// <summary>
        /// 即从哪个Controller来获取。也是Model的名字，这个Resource的主要Model来源,归谁管
        /// </summary>
        public string ControllerModelName { get; internal set; } = null!;

        //public string? Parent1ModelName { get; internal set; }

        //public string? Parent2ModelName { get; internal set; }

        //public MethodInfo? Parent1ResIdGetMethod { get; internal set; }

        //public MethodInfo? Parent2ResIdGetMethod { get; internal set; }


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
