using System;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ResEndpointAttribute : Attribute
    {
        public ResEndpointType Type { get; set; }

        public string? ResName { get; set; }

        public string? ControllerOrPlainUrl { get; set; }

        public ApiRequestAuth? DefaultReadAuth { get; set; }

        public ApiRequestAuth? DefaultWriteAuth { get; set; }

        public ResEndpointAttribute() { }

    }
}
