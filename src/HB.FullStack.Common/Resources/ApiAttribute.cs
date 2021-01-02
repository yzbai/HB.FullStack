using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.Resources
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ApiAttribute : Attribute
    {
        public string EndPointName { get; }

        public string Version { get; }

        public ApiAttribute(string endPointName, string version)
        {
            EndPointName = endPointName;
            Version = version;
        }
    }
}
