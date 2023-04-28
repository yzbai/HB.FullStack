using System;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.ApiClient
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ResEndpointAttribute : Attribute
    {
        public ResEndpointType? Type { get; set; }

        public string? ResName { get; set; }

        public string? ControllerOrPlainUrl { get; set; }

        public ApiRequestAuth? DefaultReadAuth { get; set; }

        public ApiRequestAuth? DefaultWriteAuth { get; set; }

        public ResEndpointAttribute()
        {

        }

        public ResEndpointAttribute(string resName)
        {
            ResName = resName;
        }
    }
}
