using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Services.IdBarriers;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    /// <summary>
    /// DELETE /Model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DeleteRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        [RequestBody]
        public T Resource { get; set; } = null!;

        public DeleteRequest(T res) : base(typeof(T).Name, ApiMethod.Delete, null, null)
        {
            Resource = res;
        }
    }
}