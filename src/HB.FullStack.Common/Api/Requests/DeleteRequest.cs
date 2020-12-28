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
        public List<T> Resources { get; set; } = new List<T>();

        public override int GetHashCode()
        {
            return ((ApiRequest)this).GetHashCode();
        }
    }
}