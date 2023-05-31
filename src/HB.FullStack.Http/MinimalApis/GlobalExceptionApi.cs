using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;


namespace HB.FullStack.Server.WebLib.MinimalApis
{
    public static class GlobalExceptionApi
    {
        public static IEndpointRouteBuilder MapGlobalException(this IEndpointRouteBuilder builder, string template)
        {
            builder.Map(template, HandleGlobalException);

            return builder;
        }

        private static IResult HandleGlobalException(HttpContext context)
        {
            IExceptionHandlerPathFeature? exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature == null)
            {
                //TODO: 完善， 记录请求Request。

                Globals.Logger.LogCritical("IExceptionHandlerPathFeature = null");

                return Results.BadRequest(ErrorCodes.ServerInternalError);
            }

            string path = exceptionHandlerPathFeature.Path;

            //TODO: Do we need so mutch detail ?

            //RouteValueDictionary? routeValues = exceptionHandlerPathFeature.RouteValues;

            //string? queryString = HttpContext.Request.QueryString.ToString();
            //string? content = null;

            //using (StreamReader bodyStream = new StreamReader(HttpContext.Request.Body))
            //{
            //    bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            //    content = await bodyStream.ReadToEndAsync().ConfigureAwait(false);
            //}

            ErrorCode errorCode = ErrorCodes.ServerInternalError;

            if (exceptionHandlerPathFeature.Error is ErrorCodeException errorCodeException)
            {
                errorCode = errorCodeException.ErrorCode;
            }

            Globals.Logger.LogError(exceptionHandlerPathFeature.Error, "GlobalExceptionController捕捉异常：RequestPath:{RequestPath}", path);

            return Results.BadRequest(errorCode);
        }
    }
}
