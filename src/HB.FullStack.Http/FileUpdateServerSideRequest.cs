using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;
using Microsoft.AspNetCore.Http;

namespace HB.FullStack.Common.Api
{
    public class FileUpdateServerSideRequest<T> : ApiRequest<T> where T : Resource
    {
        public FileUpdateServerSideRequest() : base(HttpMethod.Put, null) { }

        [Required]
        public T Resource { get; set; } = null!;

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
