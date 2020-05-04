using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Client.TCaptcha
{
    public class TCaptchaContext
    {
        public bool IsSucess { get; set; }

        public string AppId { get; set; }

        public string Ticket { get; set; }

        public string RandStr { get; set; }

        public Exception? Exception { get; set; }

        public TCaptchaContext(string appId, string ticket, string randstr)
        {
            AppId = appId;
            Ticket = ticket;
            RandStr = randstr;
        }
    }
}
