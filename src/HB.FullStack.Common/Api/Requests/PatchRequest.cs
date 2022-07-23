using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public sealed class PatchRequest<T> : ApiRequest where T : ApiResource
    {
        public IList<(string propertyName, string? oldValue, string? newValue)> PropertyOldNewValues { get; set; } = new List<(string, string?, string?)>();

        [OnlyForJsonConstructor]
        public PatchRequest() { }

        public PatchRequest(ApiRequestAuth auth) : base(typeof(T).Name, ApiMethodName.Patch, auth, null) { }

        public PatchRequest<T> AddProperty(string propertyName, object? oldValue, object? newValue)
        {
            PropertyOldNewValues.Add((propertyName, TypeStringConverter.ConvertToString(oldValue), TypeStringConverter.ConvertToString(newValue)));
            return this;
        }
    }
}