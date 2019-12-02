using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Mobile.TCaptcha
{
    public class TCaptchaOptions : IOptions<TCaptchaOptions>
    {
        public TCaptchaOptions Value {
            get {
                return this;
            }
        }

        public string SmsUsedAppId { get; set; }
    }
}
