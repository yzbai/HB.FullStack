using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    public class DeleteRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        public DeleteRequest(IEnumerable<T> ress) : base(HttpMethod.Delete, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethod.Delete, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(T res) : this(new T[] { res }) { }

        public DeleteRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"DeleteRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
        }

        protected override HashCode GetChildHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(typeof(DeleteRequest<T>).FullName);

            foreach (T item in Resources)
            {
                hash.Add(item);
            }

            return hash;
        }
    }
}