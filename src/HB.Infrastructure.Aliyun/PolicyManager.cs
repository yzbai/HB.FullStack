using Aliyun.Acs.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Text;
using ClientException = Aliyun.Acs.Core.Exceptions.ClientException;

namespace HB.Infrastructure.Aliyun
{
    public static class PolicyManager
    {
        public static AsyncRetryPolicy SendSmsRetryPolicy(ILogger logger)
        {
            return Policy
                .Handle<ClientException>()
                .WaitAndRetryAsync(
                    new[] { TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) },
                    (exception, timeSpan, retryCount, context) =>
                    {
                        ClientException cex = (ClientException)exception;
                        logger.LogError(exception, "Aliyun Sms Service went Wrong. Code:{0}, Msg:{1}, Type:{2}, Msg:{3}", cex.ErrorCode, cex.ErrorMessage, cex.ErrorType.ToString(), cex.Message);
                    });
        }
    }
}
