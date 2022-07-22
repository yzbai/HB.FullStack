using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Common.Api
{

    [ModelBinder(BinderType = typeof(FileUpdateServerSideRequestModelBinder))]
    public class UploadServerSideRequest<T> : PatchRequest<T> where T : ApiResource
    {

        public UploadServerSideRequest()
        {

        }

        public UploadServerSideRequest(string resName, ApiRequestAuth auth, string? condition) : base(resName, auth, condition) { }

        [CollectionNotNullOrEmpty]
        public IEnumerable<IFormFile> Files { get; set; } = null!;
    }
}
