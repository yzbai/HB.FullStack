namespace System
{
    /// <summary>
    /// 绑定到Endpoint
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EndpointBindingAttribute : Attribute
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
        /// 即从哪个Controller来获取。也是Model的名字，这个Resource的主要Model来源,归谁管
        /// </summary>
        public string ControllerModelName { get; }

        /// <summary>
        /// 属于谁的子资源
        /// </summary>
        //public string? Parent1ModelName { get; set; }

        //public string? Parent2ModelName { get; set; }

        public EndpointBindingAttribute(string endPointName, string version, string controllerModelName)
        {
            EndPointName = endPointName;
            Version = version;
            ControllerModelName = controllerModelName;
        }
    }
}
