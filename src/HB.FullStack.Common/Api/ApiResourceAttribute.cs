using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ApiResourceAttribute : Attribute
    {
        public string EndPointName { get; }

        public string Version { get; }

        public string ResName { get; }

        public string? ParentResName { get; }

        public ApiResourceAttribute(string endPointName, string version, string resName, string? parentResName)
        {
            EndPointName = endPointName;
            Version = version;
            ResName = resName;
            ParentResName = parentResName;
        }
    }
}
