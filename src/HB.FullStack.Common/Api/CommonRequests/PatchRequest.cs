using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Common.Api
{
    public sealed class PatchRequest<T> : ApiRequest where T : ApiResource
    {
        /// <summary>
        /// 将PropertyValue转换成字符串
        /// </summary>
        [RequestBody]
        public ChangedPack RequestData { get; set; } = new ChangedPack();

        public PatchRequest() : base(typeof(T).Name, ApiMethod.UpdateProperties, null, null) { }

        public PatchRequest<T> AddProperty(string propertyName, object? oldValue, object? newValue)
        {
            RequestData.ChangedProperties.Add(new ChangedProperty(propertyName, oldValue, newValue));

            return this;
        }
    }
}