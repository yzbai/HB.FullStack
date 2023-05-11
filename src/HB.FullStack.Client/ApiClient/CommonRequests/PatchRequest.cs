using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Client.ApiClient
{
    public sealed class PatchRequest<T> : ApiRequest where T : SharedResource
    {
        [RequestBody]
        public PropertyChangeJsonPack RequestData { get; set; }

        public PatchRequest(PropertyChangeJsonPack requestData) : base(typeof(T).Name, ApiMethod.UpdateProperties, null, null)
        {
            RequestData = requestData;
        }
    }
}