using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public class GetRequest : ApiRequest
    {

        [RequestQuery]
        public IList<string> Ids { get; } = new List<string>();

        [RequestQuery]
        public int? Page { get; set; }

        [RequestQuery]
        public int? PerPage { get; set; }

        [RequestQuery]
        public string? OrderBys { get; set; }

        /// <summary>
        /// 包含哪些子资源
        /// </summary>
        [RequestQuery]
        public IList<string> ResIncludes { get; } = new List<string>();

        /// <summary>
        /// 只支持PropertyName=PropertyStringValue这种Equal形式。其他形式请自行定义GetByCondition Request
        /// </summary>
        [RequestQuery]
        public IList<PropertyFilter> PropertyFilters { get; } = new List<PropertyFilter>();

        public GetRequest(string resName) : base(resName, ApiMethod.Get, null, null) { }
    }

    public class GetRequest<T> : GetRequest where T : ApiResource
    {
        public GetRequest() : base(typeof(T).Name) { }
    }
}