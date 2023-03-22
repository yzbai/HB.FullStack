﻿using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Common.Api
{
    public sealed class PatchRequest<T> : ApiRequest where T : ApiResource
    {
        /// <summary>
        /// 将PropertyValue转换成字符串
        /// </summary>
        [RequestBody]
        public PropertyChangePack? RequestData { get; set; }

        public PatchRequest() : base(typeof(T).Name, ApiMethod.UpdateProperties, null, null) { }
    }
}