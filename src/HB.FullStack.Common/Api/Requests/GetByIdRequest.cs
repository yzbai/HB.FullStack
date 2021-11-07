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

    public class GetByIdRequest<T, TSub> : GetRequest<T,TSub> where T : ApiResource2 where TSub:ApiResource2
    {
        [JsonIgnore]
        [NoEmptyGuid]
        public Guid SubId { get; private set; }

        public GetByIdRequest(Guid id, Guid subId):base(id, null)
        {
            SubId = subId;
        }

        public GetByIdRequest(string apiKeyName, Guid id, Guid subId) : base(id, apiKeyName, null)
        {
            SubId=subId;
        }

        public override string ToDebugInfo()
        {
            return $"GetByIdRequest. Resource:{typeof(T).FullName}, Id:{Id}. SubResource:{typeof(TSub).FullName}, SubId:{SubId}";
        }

        protected override string GetUrlCore()
        {
            string url = $"{ApiVersion}/{ResName}/{Id}/{SubResName}/{SubId}";

            return AddCommonQueryToUrl(url);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), SubId);
        }
    }
}
