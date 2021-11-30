using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

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

        public UpdateRequest(IEnumerable<T> ress) : base(HttpMethodName.Put, null)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethodName.Put, null)
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

    public class UpdateRequest2<T, TOwner> : UpdateRequest<T> where T : ApiResource2 where TOwner : ApiResource2
    {
        /// <summary>
        /// 主要Resource 的ID
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public string OwnerResName { get; set; } = null!;
        public UpdateRequest2(Guid ownerId, IEnumerable<T> ress) : base(ress)
        {
            ApiResourceDef ownerDef = ApiResourceDefFactory.Get<TOwner>();
            OwnerId = ownerId;
            OwnerResName = ownerDef.ResName;
        }

        public UpdateRequest2(Guid ownerId, T res) : this(ownerId, new T[] { res }) { }

        public override string ToDebugInfo()
        {
            return $"UpdateRequest, ApiResourceType:{typeof(T).Name}, OwnerResourceType:{typeof(TOwner).Name},  Resources:{SerializeUtil.ToJson(Resources)}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), OwnerId, OwnerResName);
        }
    }
}