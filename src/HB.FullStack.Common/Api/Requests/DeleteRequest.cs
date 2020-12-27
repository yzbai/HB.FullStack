using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class DeleteRequest<T> : ApiRequest<T> where T : Resource
    {
        public DeleteRequest() : base(HttpMethod.Delete, null) { }

        public DeleteRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Delete, null) { }

        [Required]
        [IdBarrier]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<T> Resources { get; set; } = new List<T>();
#pragma warning restore CA2227 // Collection properties should be read only

        public override int GetHashCode()
        {
            return ((ApiRequest)this).GetHashCode();
        }
    }
}