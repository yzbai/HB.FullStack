using Aliyun.Acs.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public static class PolicyManager
    {
        public static RetryPolicy Default(ILogger logger)
        {
            return Policy
                .Handle<ClientException>()
                .WaitAndRetry(
                    new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(16) },
                    (exception, timeSpan, retryCount, context) =>
                    {
                        ClientException cex = (ClientException)exception;
                        logger.LogError(exception, "Aliyun Sms Service went Wrong. Code:{0}, Msg:{1}, Type:{2}, Msg:{3}", cex.ErrorCode, cex.ErrorMessage, cex.ErrorType.ToString(), cex.Message);
                    });
        }
    }
}
