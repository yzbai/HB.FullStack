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

        public DeleteRequest(IEnumerable<T> ress) : base(HttpMethodName.Delete, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethodName.Delete, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(T res) : this(new T[] { res }) { }

        public DeleteRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"DeleteRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
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

    public class DeleteRequest<T, TSub> : ApiRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<TSub> SubResources { get; } = new List<TSub>();

        public DeleteRequest(Guid id, IEnumerable<TSub> ress) : base(id, HttpMethodName.Delete, null)
        {
            SubResources.AddRange(ress);
        }

        public DeleteRequest(string apiKeyName, Guid id, IEnumerable<TSub> ress) : base(id, apiKeyName, HttpMethodName.Delete, null)
        {
            SubResources.AddRange(ress);
        }

        public DeleteRequest(Guid id, TSub res) : this(id, new TSub[] { res }) { }

        public DeleteRequest(string apiKeyName, Guid id, TSub res) : this(apiKeyName, id, new TSub[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"DeleteRequest, ApiResourceType:{typeof(T).Name}, SubResourceType:{typeof(TSub).Name},  Resources:{SerializeUtil.ToJson(SubResources)}";
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