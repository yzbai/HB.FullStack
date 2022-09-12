using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Common.Api
{

    [ModelBinder(BinderType = typeof(FileUpdateServerSideRequestModelBinder))]
    public class UploadServerSideRequest<T> : ApiRequest where T : ApiResource
    {
        public UploadServerSideRequest(string resName, ApiRequestAuth2 auth, string? condition) : base(resName, ApiMethod.UpdateFields, auth, condition) { }

        [CollectionNotNullOrEmpty]
        public IEnumerable<IFormFile> Files { get; set; } = null!;
    }
}
