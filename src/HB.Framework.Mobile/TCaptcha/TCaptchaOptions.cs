using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Client.TCaptcha
{
    public class TCaptchaOptions : IOptions<TCaptchaOptions>
    {
        public TCaptchaOptions Value {
            get {
                return this;
            }
        }

        public string SmsUsedAppId { get; private set; } = default!;


    }
}
