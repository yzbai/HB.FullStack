using System;
using System.Net.Http;

namespace HB.FullStack.Common.Api.Requests
{
    /// <summary>
    /// GET /Version/ResourceCollectionName?Page=1&PerPage=100&OrderBy=Name,Age
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GetAllRequest<T> : GetRequest<T> where T : ApiResource2
    {
        public GetAllRequest() : base(null)
        {
        }

        public GetAllRequest(string apiKeyName) : base(apiKeyName, null)
        {
        }

        protected override string CreateUrl()
        {
            return CreateDefaultUrl();
        }

        public override string ToDebugInfo()
        {
            return $"GetAllRequest. Resource:{typeof(T).FullName}";
        }

        protected override HashCode GetHashCodeCore()
        {
            HashCode code = new HashCode();

            code.Add(typeof(GetAllRequest<T>).FullName);

            return code;
        }
    }
}
