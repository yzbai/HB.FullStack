using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HB.Component.Authorization.Abstractions
{
    public class SignOutContext
    {
        public HttpContext HttpContext { get; set; }
    }
}
