using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; } = new List<T>();

        public AddRequest() : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Post, true, ApiAuthType.Jwt, null)) { }

        public AddRequest(IEnumerable<T> ress, HttpRequestBuilder httpRequestBuilder) : base(httpRequestBuilder)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(IEnumerable<T> ress) : this(ress, new RestfulHttpRequestBuilder<T>(HttpMethodName.Post, true, ApiAuthType.Jwt, null)) { }

        public AddRequest(string apiKeyName, IEnumerable<T> ress) : this(ress, new RestfulHttpRequestBuilder<T>(HttpMethodName.Post, true, apiKeyName, null)) { }

        public AddRequest(T res) : this(new T[] { res }) { }

        public AddRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(base.GetHashCode());

            foreach (T item in Resources)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }
    }
}