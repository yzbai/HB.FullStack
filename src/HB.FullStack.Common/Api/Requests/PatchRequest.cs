using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PatchRequest<T> : ApiRequest where T : ApiResource
    {
        public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();

        [OnlyForJsonConstructor]
        public PatchRequest() { }

        public PatchRequest(string resName, ApiRequestAuth auth) : base(resName, ApiMethodName.Patch, auth, null) { }

        public PatchRequest<T> AddProperty(string propertyName, object? propertyValue)
        {
            Properties[propertyName] = propertyValue;
            return this;
        }
    }
}