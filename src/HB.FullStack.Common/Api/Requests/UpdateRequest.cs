using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class UpdateRequest<T> : ApiRequest<T> where T : Resource
    {
        public UpdateRequest() : base(HttpMethod.Put, null) { }

        public UpdateRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Put, null) { }

        [IdBarrier]
        [Required]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<T> Resources { get; set; } = new List<T>();
#pragma warning restore CA2227 // Collection properties should be read only

        public override int GetHashCode()
        {
            return ((ApiRequest)this).GetHashCode();
        }
    }
}