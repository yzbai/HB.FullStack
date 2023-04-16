using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Services.IdBarriers;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    /// <summary>
    /// POST /Model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AddRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        [RequestBody]
        public T Resource { get; set; } = null!;

        public AddRequest(T res) : base(typeof(T).Name, ApiMethod.Add, null, null)
        {
            Resource = res;
        }
    }
}