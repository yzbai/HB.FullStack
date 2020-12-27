using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest<T> where T : Resource
    {
        public AddRequest() : base(HttpMethod.Post, null) { }

        public AddRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Post, null) { }

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