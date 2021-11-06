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

    public class UpdateRequest<T, TSub> : ApiRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<TSub> SubResources { get; } = new List<TSub>();

        public UpdateRequest(Guid id, IEnumerable<TSub> ress) : base(id, HttpMethod.Put, null)
        {
            SubResources.AddRange(ress);
        }

        public UpdateRequest(string apiKeyName, Guid id, IEnumerable<TSub> ress) : base(id, apiKeyName, HttpMethod.Put, null)
        {
            SubResources.AddRange(ress);
        }

        public UpdateRequest(Guid id, TSub res) : this(id, new TSub[] { res }) { }

        public UpdateRequest(string apiKeyName, Guid id, TSub res) : this(apiKeyName, id, new TSub[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"UpdateRequest, ApiResourceType:{typeof(T).Name}, SubResourceType:{typeof(TSub).Name},  Resources:{SerializeUtil.ToJson(SubResources)}";
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(base.GetHashCode());

            foreach (TSub item in SubResources)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }
        //TODO: 核查各个Hashcode

    }
}