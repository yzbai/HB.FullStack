using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public sealed class PatchRequest<T> : ApiRequest where T : ApiResource
    {
        public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();

        [OnlyForJsonConstructor]
        public PatchRequest() { }

        public PatchRequest(ApiRequestAuth auth) : base(typeof(T).Name, ApiMethodName.Patch, auth, null) { }

        public PatchRequest<T> AddProperty(string propertyName, object? propertyValue)
        {
            Properties[propertyName] = propertyValue;
            return this;
        }
    }
}