using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.MauiLib
{
    public static class MauiExceptions
    {
        
        internal static Exception NoInternet()
        {
            MauiException exception = new MauiException(ErrorCodes.NoInternet, "NoInternet");
            return exception;
        }


        internal static Exception CaptchaErrorReturn(string? captcha, ApiRequest request)
        {
            MauiException ex = new MauiException(ErrorCodes.CaptchaErrorReturn, "CaptchaErrorReturn");
            ex.Data["Captcha"] = captcha;
            ex.Data["RequestUrl"] = request;

            return ex;
        }


    }
}
