using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public sealed class PatchRequest<T> : ApiRequest where T : ApiResource
    {
        /// <summary>
        /// 将PropertyValue转换成字符串
        /// </summary>
        [RequestBody]
        public IList<ChangedProperty> ChangedProperties { get; set; } = new List<ChangedProperty>();

        public PatchRequest() : base(typeof(T).Name, ApiMethod.UpdateFields, null, null) { }

        public PatchRequest<T> AddProperty(string propertyName, object? oldValue, object? newValue)
        {
            ChangedProperties.Add(new ChangedProperty
            {
                PropertyName = propertyName,
                PropertyOldStringValue = oldValue?.ToString(),
                PropertyNewStringValue = newValue?.ToString(),
            });

            return this;
        }
    }
}