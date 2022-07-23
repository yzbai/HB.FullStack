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
        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string? OrderBys { get; set; }

        /// <summary>
        /// 包含哪些子资源
        /// </summary>
        public string? Includes { get; set; }

        public IList<PropertyFilter> PropertyFilters { get; } = new List<PropertyFilter>();

        public GetRequest(string resName) : base(resName, ApiMethodName.Get, null, null) { }
    }

    public sealed class GetRequest<T> : GetRequest where T : ApiResource
    {
        public GetRequest() : base(typeof(T).Name) { }
    }
}