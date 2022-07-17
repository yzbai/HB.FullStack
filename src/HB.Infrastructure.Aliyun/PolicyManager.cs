using System;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;

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
                        logger.LogError(exception, "Aliyun Sms Service went Wrong. Code:{ErrorCode}, Msg:{ErrorMessage}, Type:{Type}, Msg:{ClientExceptionMessage}", cex.ErrorCode, cex.ErrorMessage, cex.ErrorType.ToString(), cex.Message);
                    });
        }
    }
}
