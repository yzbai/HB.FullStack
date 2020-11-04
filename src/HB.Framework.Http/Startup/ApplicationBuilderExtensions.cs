﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Common.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionController(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler("/exception");
        }

        public static IApplicationBuilder UseOnlyHttps(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
             {
                 if (!context.Request.IsHttps)
                 {
                     ApiError errorResponse = new ApiError(ServerErrorCode.ApiHttpsRequired);

                     context.Response.StatusCode = 400;
                     context.Response.ContentType = "application/json";
                     await context.Response.WriteAsync(SerializeUtil.ToJson(errorResponse)).ConfigureAwait(false);
                 }
                 else
                 {
                     await next().ConfigureAwait(false);
                 }

             });
        }
    }
}
