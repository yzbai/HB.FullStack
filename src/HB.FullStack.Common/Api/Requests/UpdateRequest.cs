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
        public IList<T> Resources { get; } = new List<T>();
    }
}