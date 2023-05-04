using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Server.WebLib
{

    [ModelBinder(BinderType = typeof(FileUpdateServerSideRequestModelBinder))]
    public class UploadServerSideRequest<T>  where T : SharedResource
    {
        public UploadServerSideRequest() { }

        [CollectionNotNullOrEmpty]
        public IEnumerable<IFormFile> Files { get; set; } = null!;
    }
}
