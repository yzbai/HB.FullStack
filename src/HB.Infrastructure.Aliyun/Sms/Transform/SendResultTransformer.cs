using Aliyun.Acs.Dysmsapi.Model.V20170525;
using HB.Component.Resource.Sms.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms.Transform
{
    public static class SendResultTransformer
    {
        public static SendResult Transform(SendSmsResponse response)
        {
            if (response == null)
            {
                return null;
            }

            SendResult sendResult = new SendResult();

            sendResult.Message = response.Message;
            sendResult.Succeeded = response.Code.Equals("ok", StringComparison.InvariantCultureIgnoreCase);

            return sendResult;
        }
    }
}
