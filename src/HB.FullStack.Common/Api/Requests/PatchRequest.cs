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
        public IList<ChangedProperty> ChangedProperties { get; set; } = new List<ChangedProperty>();

        [OnlyForJsonConstructor]

        public PatchRequest() : base(typeof(T).Name, ApiMethodName.Patch, null, null) { }

        public PatchRequest<T> AddProperty(string propertyName, object? oldValue, object? newValue)
        {
            ChangedProperties.Add(new ChangedProperty
            {
                PropertyName = propertyName,
                PropertyOldStringValue = TypeStringConverter.ConvertToString(oldValue),
                PropertyNewStringValue = TypeStringConverter.ConvertToString(newValue)
            });

            return this;
        }
    }
}