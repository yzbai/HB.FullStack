using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Web
{

    [ModelBinder(BinderType = typeof(FileUpdateServerSideRequestModelBinder))]
    public class UploadServerSideRequest<T>  where T : ApiResource
    {
        public UploadServerSideRequest() { }

        [CollectionNotNullOrEmpty]
        public IEnumerable<IFormFile> Files { get; set; } = null!;
    }
}
