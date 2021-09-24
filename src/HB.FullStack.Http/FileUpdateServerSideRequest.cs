using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Common.Api
{

    [ModelBinder(BinderType = typeof(FileUpdateServerSideRequestModelBinder))]
    public class FileUpdateServerSideRequest<T> : UpdateRequest<T> where T : ModelObject2
    {
        [Required]
        public IEnumerable<IFormFile> Files { get; set; } = null!;

        public override int GetHashCode()
        {
            return HashCode.Combine(Resources);
        }
    }
}
