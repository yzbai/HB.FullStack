using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// GET /Model
    /// </summary>
    public class GetRequest : ApiRequest
    {
        public string? Id { get; set; }

        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string? OrderBys { get; set; }

        /// <summary>
        /// 包含哪些子资源
        /// </summary>
        public string? Includes { get; set; }

        /// <summary>
        /// 只支持PropertyName=PropertyStringValue这种Equal形式。其他形式请自行定义GetByCondition Request
        /// </summary>
        public IList<PropertyFilter> PropertyFilters { get; } = new List<PropertyFilter>();

        public GetRequest(string resName) : base(resName, ApiMethodName.Get, null, null) { }
    }

    public sealed class GetRequest<T> : GetRequest where T : ApiResource
    {
        public GetRequest() : base(typeof(T).Name) { }
    }
}