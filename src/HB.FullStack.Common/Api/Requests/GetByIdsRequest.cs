using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [NoEmptyGuid]
        public IList<Guid> Ids { get; set; } = new List<Guid>();

        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public GetByIdsRequest()
        { }

        public GetByIdsRequest(params Guid[] ids) : base(HttpMethodName.Get, "ByIds")
        {
            ids.AddRange(ids);
        }
    }
}