using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Filters
{

    public sealed class RequireSmsValidationCodeAttribute : ActionFilterAttribute
    {
        public string SmsCodeParameterName { get; set; }
        
        public string SmsCodeSessiionName { get; set; }

        public RequireSmsValidationCodeAttribute(string parameterName) : this(parameterName, parameterName) { }

        public RequireSmsValidationCodeAttribute(string smsCodeParameterName, string smsCodeSessionName)
        {
            SmsCodeParameterName = smsCodeParameterName;
            SmsCodeSessiionName = smsCodeSessionName;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (handleValidationAsync(context.HttpContext))
            {
                base.OnActionExecuting(context);
            }
            else
            {
                handleFailRequest(context);
            }
        }

        private void handleFailRequest(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                context.Result = new BadRequestObjectResult("Sms Error.");
            }
            else
            {
                //string returnUrl = context.HttpContext.Request.Query[_options.ReturnUrlParameter];

                //if (string.IsNullOrEmpty(returnUrl))
                //{
                //    string url = QueryHelpers.AddQueryString(_options.RedirectUrl, _options.ReturnUrlParameter, context.HttpContext.Request.GetDisplayUrl());

                //    context.Result = new RedirectResult(_options.RedirectUrl);
                //}
                //else
                //{
                //    context.Result = new RedirectResult(returnUrl);
                //}

                context.Result = new RedirectResult("~/Error");
            }
        }

        private bool handleValidationAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            string code = httpContext.GetValueFromRequest(SmsCodeParameterName);

            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            return code.Equals(httpContext.Session.GetString(SmsCodeSessiionName));
        }
    }
}
