using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HB.Framework.Common.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HB.Framework.Http.Filters
{
    public class CheckFileAttribute : ActionFilterAttribute
    {
        private readonly string _filePropertyName;
        private readonly int _maxSize;
        private readonly IList<string> _allowedExtensions;

        public CheckFileAttribute(string filePropertyName, IList<string> allowedExtensions, int maxSize = 2097152)
        {
            _filePropertyName = filePropertyName;
            _maxSize = maxSize;
            _allowedExtensions = new List<string>(allowedExtensions);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (context.HttpContext.Request.ContentType != "multipart/form-data")
            {
                OnError(context, ApiErrorCode.FileUploadMultipartFormDataNeeded);
                return;
            }

            if (!context.ActionArguments.ContainsKey(_filePropertyName))
            {
                OnError(context, ApiErrorCode.FileUploadNeeded);
                return;
            }

            if (!(context.ActionArguments[_filePropertyName] is IFormFile file))
            {
                OnError(context, ApiErrorCode.FileUploadNeeded);
                return;
            }

            string fileExtension = Path.GetExtension(file.FileName);

            if (fileExtension.IsNullOrEmpty() || !_allowedExtensions.Contains(fileExtension))
            {
                OnError(context, ApiErrorCode.FileUploadTypeNotMatch);
                return;
            }


        }

        private static void OnError(ActionExecutingContext context, ApiErrorCode error)
        {
            if (context != null)
            {
                context.Result = new BadRequestObjectResult(new ApiError(error));
            }
        }
    }
}
