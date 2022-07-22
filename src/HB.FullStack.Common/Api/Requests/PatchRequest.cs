using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PatchRequest<T> : ApiRequest where T : ApiResource
    {
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        [OnlyForJsonConstructor]
        protected PatchRequest() { }

        protected PatchRequest(string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Patch, auth, condition) { }
    }
}