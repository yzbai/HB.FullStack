using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Common.Api
{

    [ModelBinder(BinderType = typeof(FileUpdateServerSideRequestModelBinder))]
    public class UploadServerSideRequest<T> : UpdateRequest<T> where T : ApiResource2
    {
        public UploadServerSideRequest() : base(Array.Empty<T>()) { }


        [Required]
        public IEnumerable<IFormFile> Files { get; set; } = null!;
    }
}
