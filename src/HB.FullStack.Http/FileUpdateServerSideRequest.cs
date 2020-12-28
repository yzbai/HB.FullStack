using System;
using System.Collections.Generic;
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
        public List<T> Resources { get; set; } = new List<T>();

        [Required]
        public IEnumerable<IFormFile> Files { get; set; } = null!;

        public override int GetHashCode()
        {
            return ((ApiRequest)this).GetHashCode();
        }
    }
}
