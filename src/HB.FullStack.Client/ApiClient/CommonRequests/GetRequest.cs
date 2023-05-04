using System.Collections.Generic;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
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

        [RequestQuery]
        public IList<string> WherePropertyNames { get; set; } = new List<string>();

        [RequestQuery]
        public IList<string?> WherePropertyValues { get; set; } = new List<string?>();

        public GetRequest(string resName) : base(resName, ApiMethod.Get, null, null) { }
    }

    public class GetRequest<T> : GetRequest where T : SharedResource
    {
        public GetRequest() : base(typeof(T).Name) { }
    }
}