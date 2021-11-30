using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Api.Requests
{
    /// <summary>
    /// GET /Version/ResourceCollection/{Id}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GetByIdRequest<T> : GetRequest<T> where T : ApiResource2
    {
        /// <summary>
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        [NoEmptyGuid]
        public Guid Id { get; private set; }

        public GetByIdRequest(Guid id) : base(null)
        {
            Id = id;
        }

        public GetByIdRequest(string apiKeyName, Guid id) : base(apiKeyName, null)
        {
            Id = id;
        }

        public override string ToDebugInfo()
        {
            return $"GetByIdRequest. Resource:{typeof(T).FullName}, Id:{Id}";
        }

        protected override string GetUrlCore()
        {
            string url = $"{ApiVersion}/{ResName}/{Id}";

            return AddCommonQueryToUrl(url);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id);
        }
    }

    public class GetByIdRequest2<T, TOwner> : GetByIdRequest<T> where T : ApiResource2 where TOwner : ApiResource2
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

        public GetByIdRequest2(Guid ownerId, Guid id) : base(id)
        {
            ApiResourceDef ownerDef = ApiResourceDefFactory.Get<TOwner>();
            OwnerId = ownerId;
            OwnerResName = ownerDef.ResName;
        }

        public override string ToDebugInfo()
        {
            return $"GetByIdRequest. Resource:{typeof(T).FullName}, Id:{Id}. OwnerResource:{typeof(TOwner).FullName}, OwnerId:{OwnerId}";
        }

        protected override string GetUrlCore()
        {
            string url = $"{ApiVersion}/{OwnerResName}/{OwnerId}/{ResName}/{Id}";

            return AddCommonQueryToUrl(url);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), OwnerId, OwnerResName);
        }
    }
}
