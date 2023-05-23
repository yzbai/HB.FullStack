using System;

namespace HB.FullStack.Client.ApiClient
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ResEndpointAttribute : Attribute
    {
        public ResEndpointType? Type { get; set; }

        public string? SiteName { get; set; }

        public string? ResName { get; set; }

        public string? ControllerOrPlainUrl { get; set; }

        public ApiRequestAuth? DefaultReadAuth { get; set; }

        public ApiRequestAuth? DefaultWriteAuth { get; set; }

        public ResEndpointAttribute()
        {
        }
    }
}