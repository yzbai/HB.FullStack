using Aliyun.Acs.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms.Transform
{
    internal static class SendResultTransformer
    {
        public static SendResult ToResult(this CommonResponse response)
        {
            if (response == null)
            {
                return null;
            }

            SendResult sendResult = new SendResult
            {
                //Message = response.Message,
                //Succeeded = response.Code.Equals("ok", GlobalSettings.ComparisonIgnoreCase)
            };

            return sendResult;
        }
    }
}
