using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Web.Middlewares
{
    public class RateLimitMiddleware : IMiddleware
    {
        private readonly ILogger<RateLimitMiddleware> _logger;

        public RateLimitMiddleware(ILogger<RateLimitMiddleware> logger)
        {
            _logger = logger;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
