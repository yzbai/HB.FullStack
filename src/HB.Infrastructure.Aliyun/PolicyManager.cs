using Aliyun.Acs.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public static class PolicyManager
    {
        public static Policy Default(ILogger logger)
        {
            return Policy
                .Handle<ServerException>()
                .Or<ClientException>()
                .WaitAndRetryAsync(
                    new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8) },
                    (exception, timeSpan, retryCount, context) =>
                    {
                        ClientException cex = (ClientException)exception;
                        logger.LogError(exception, "Code:{0}, Msg:{1}, Type:{2}, Msg:{3}", cex.ErrorCode, cex.ErrorMessage, cex.ErrorType.GetDescription(), cex.Message);
                    });
        }
    }
}
