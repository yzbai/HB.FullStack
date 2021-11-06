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

    public class AddRequest<T, TSub> : ApiRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<TSub> SubResources { get; } = new List<TSub>();

        public AddRequest(Guid id, IEnumerable<TSub> ress) : base(id, HttpMethod.Post, null)
        {
            SubResources.AddRange(ress);
        }

        public AddRequest(string apiKeyName, Guid id, IEnumerable<TSub> ress) : base(id, apiKeyName, HttpMethod.Post, null)
        {
            SubResources.AddRange(ress);
        }

        public AddRequest(Guid id, TSub res) : this(id, new TSub[] { res }) { }

        public AddRequest(string apiKeyName, Guid id, TSub res) : this(apiKeyName, id, new TSub[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"AddRequest, ApiResourceType:{typeof(T).Name}, SubResourceType:{typeof(TSub).Name},  Resources:{SerializeUtil.ToJson(SubResources)}";
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