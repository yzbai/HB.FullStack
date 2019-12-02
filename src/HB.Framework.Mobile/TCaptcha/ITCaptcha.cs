using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Mobile.TCaptcha
{
    public interface ITCaptcha
    {
        void ShowSmsCaptchaDialog(Func<TCaptchaContext, Task> callback);
    }
}
