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

        protected override string CreateUrl()
        {
            return $"{CreateDefaultUrl()}/{Id}";
        }

        protected override HashCode GetHashCodeCore()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(typeof(GetByIdRequest<T>).FullName);
            hashCode.Add(Id);

            return hashCode;
        }
    }
}
