using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Components.IdBarriers;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
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