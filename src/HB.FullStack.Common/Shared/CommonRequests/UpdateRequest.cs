using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared.Attributes;

namespace HB.FullStack.Common.Shared.CommonRequests
{
    public sealed class UpdateRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        [RequestBody]
        public T Resource { get; set; } = null!;

        public UpdateRequest(T res) : base(typeof(T).Name, ApiMethod.Update, null, null)
        {
            Resource = res;
        }
    }
}