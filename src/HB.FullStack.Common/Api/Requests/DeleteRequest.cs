using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class DeleteRequest<T> : ApiRequest where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public DeleteRequest() { }

        public DeleteRequest(IEnumerable<T> ress, HttpRequestBuilder httpRequestBuilder) : base(httpRequestBuilder)
        {
            Resources.AddRange(ress);
        }
        public DeleteRequest(IEnumerable<T> ress) : this(ress, new RestfulHttpRequestBuilder<T>(HttpMethodName.Delete, true, null)) { }

        public DeleteRequest(T res) : this(new T[] { res }) { }

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