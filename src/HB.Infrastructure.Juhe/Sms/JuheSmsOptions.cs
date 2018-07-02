using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace HB.Infrastructure.Juhe.Sms
{
    public class TemplateSetting
    {
        public long SignatureId { get; set; } = -1;
        public string Signagure { get; set; }
        public long TemplateId { get; set; } = -1;
        public string Template { get; set; }
        public string ApiBaseUrl { get; set; }
    }

    public class SmsSecuritySettings
    {
        public int IPMaxCountOfHour { get; set; }

        public int IPMaxCountOfDay { get; set; }

        public int MobileIntervalSeconds { get; set; }

        public int MobileMaxCountOfHour { get; set; }

        public int MobileMaxCountOfDay { get; set; }
    }

    public class JuheSmsOptions : IOptions<JuheSmsOptions>
    {
        public JuheSmsOptions Value { get { return this; } }

        public string AppKey { get; set; }
        public int CodeLength { get; set; }

        public string MobileParameter { get; set; }
        public string MobileCodeParameter { get; set; }

        public TimeSpan ExpireTimeRange { get; set; }

        public List<TemplateSetting> TemplateSettings { get; set; }

        public SmsSecuritySettings SecuritySettings { get; set; }
    }

    

}