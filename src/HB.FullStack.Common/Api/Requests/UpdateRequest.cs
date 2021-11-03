using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// PUT /Ver/ResoruceCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [IdBarrier]
        [CollectionMemeberValidated]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        public UpdateRequest(IEnumerable<T> ress) : base(HttpMethod.Put, null)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethod.Put, null)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(T res) : this(new T[] { res }) { }

        public UpdateRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"UpdateRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
        }

        protected override HashCode GetChildHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(typeof(UpdateRequest<T>).FullName);

            foreach (T item in Resources)
            {
                hash.Add(item);
            }

            return hash;
        }
    }

    //TODO: 考虑Sub资源  UpdateRequest<T,TSub>
}