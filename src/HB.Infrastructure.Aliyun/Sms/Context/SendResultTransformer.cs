using Aliyun.Acs.Dysmsapi.Model.V20170525;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms.Transform
{
    public static class SendResultTransformer
    {
        public static SendResult ToResult(this SendSmsResponse response)
        {
            if (response == null)
            {
                return null;
            }

            SendResult sendResult = new SendResult
            {
                Message = response.Message,
                Succeeded = response.Code.Equals("ok", GlobalSettings.ComparisonIgnoreCase)
            };

            return sendResult;
        }
    }
}
