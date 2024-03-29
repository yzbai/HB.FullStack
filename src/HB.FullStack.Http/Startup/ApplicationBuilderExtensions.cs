﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using HB.FullStack.Server;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionController(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler($"/{WebConsts.GlobalException}");
        }

        public static IApplicationBuilder UseOnlyHttps(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
             {
                 if (!context.Request.IsHttps)
                 {
                     context.Response.StatusCode=400;
                     context.Response.ContentType="application/json";
                     await context.Response.WriteAsync(SerializeUtil.ToJson(ErrorCodes.HttpsRequired)).ConfigureAwait(false);
                 }
                 else
                 {
                     await next().ConfigureAwait(false);
                 }

             });
        }
    }
}
