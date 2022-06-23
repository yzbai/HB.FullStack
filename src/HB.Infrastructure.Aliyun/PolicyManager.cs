﻿using Aliyun.Acs.Core.Exceptions;
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
        public static RetryPolicy Default(ILogger logger)
        {
            return Policy
                .Handle<ClientException>()
                .WaitAndRetry(
                    new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2) },
                    (exception, timeSpan, retryCount, context) =>
                    {
                        ClientException cex = (ClientException)exception;
                        logger.LogError(exception, "Aliyun Sms Service went Wrong. Code:{0}, Msg:{1}, Type:{2}, Msg:{3}", cex.ErrorCode, cex.ErrorMessage, cex.ErrorType.ToString(), cex.Message);
                    });
        }
    }
}
