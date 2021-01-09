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

        public ApiResourceAttribute(string endPointName, string version)
        {
            EndPointName = endPointName;
            Version = version;
        }
    }
}
