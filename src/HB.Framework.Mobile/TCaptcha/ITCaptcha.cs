using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Client.TCaptcha
{
    public interface ITCaptcha
    {
        void ShowSmsCaptchaDialog(Func<TCaptchaContext, Task> callback);
    }
}
