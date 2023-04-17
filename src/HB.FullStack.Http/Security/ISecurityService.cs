using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HB.FullStack.Server.WebLib.Security
{
    public interface ISecurityService
    {
        Task<bool> NeedPublicResourceTokenAsync(FilterContext context);

        
        Task<byte[]> ProcessFormFileAsync(IFormFile? formFile, string[] permittedFileSuffixes, long sizeLimit);
    }
}
