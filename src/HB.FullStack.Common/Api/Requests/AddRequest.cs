using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; } = new List<T>();

        public AddRequest(IEnumerable<T> ress) : base(HttpMethod.Post, null)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethod.Post, null)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(T res) : this(new T[] { res }) { }

        public AddRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"AddRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
        }

        protected override HashCode GetChildHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(typeof(AddRequest<T>).FullName);

            foreach (T item in Resources)
            {
                hash.Add(item);
            }

            return hash;
        }
    }

}