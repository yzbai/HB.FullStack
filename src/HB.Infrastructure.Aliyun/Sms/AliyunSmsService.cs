﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Aliyun.Acs.Core;
using System.Threading.Tasks;
using Aliyun.Acs.Core.Http;
using System;
using Microsoft.Extensions.Caching.Distributed;
using HB.Framework.Common.Validate;

namespace HB.Infrastructure.Aliyun.Sms
{
    internal class AliyunSmsService : IAliyunSmsService
    {
        private readonly AliyunSmsOptions _options;
        private readonly IAcsClient _client;
        private readonly ILogger _logger;
        private readonly IDistributedCache _cache;

        public AliyunSmsService(IOptions<AliyunSmsOptions> options, ILogger<AliyunSmsService> logger, IDistributedCache cache)
        {
            _options = options.Value;
            _logger = logger;
            _cache = cache;

            AliyunUtil.AddEndpoint(ProductNames.SMS, _options.RegionId, _options.Endpoint);
            _client = AliyunUtil.CreateAcsClient(_options.RegionId, _options.AccessKeyId, _options.AccessKeySecret);
            
        }

        public Task<SendResult> SendValidationCode(string mobile/*, out string smsCode*/)
        {
            string smsCode = GenerateNewSmsCode(_options.TemplateIdentityValidation.CodeLength);

            string templateParam = string.Format(GlobalSettings.Culture, "{{\"{0}\":\"{1}\", \"{2}\":\"{3}\"}}",
                    _options.TemplateIdentityValidation.ParamCode,
                    smsCode,
                    _options.TemplateIdentityValidation.ParamProduct,
                    _options.TemplateIdentityValidation.ParamProductValue);

            CommonRequest request = new CommonRequest {
                Method = MethodType.POST,
                Domain = "dysmsapi.aliyuncs.com",
                Version = "2017-05-25",
                Action = "SendSms"
            };

            request.AddQueryParameters("PhoneNumbers", mobile);
            request.AddQueryParameters("SignName", _options.SignName);
            request.AddQueryParameters("TemplateCode", _options.TemplateIdentityValidation.TemplateCode);
            request.AddQueryParameters("TemplateParam", templateParam);

            string cachedSmsCode = smsCode;

            return PolicyManager.Default(_logger).ExecuteAsync(async () => {
                Task<CommonResponse> task = new Task<CommonResponse>(() => _client.GetCommonResponse(request));
                task.Start(TaskScheduler.Default);

                CommonResponse response = await task.ConfigureAwait(false);
                SendResult sendResult = SerializeUtil.FromJson<SendResult>(response.Data);

                if (sendResult.IsSuccessful())
                {
                    CacheSmsCode(mobile, cachedSmsCode, _options.TemplateIdentityValidation.ExpireMinutes);
                }
                else
                {
                    _logger.LogCritical($"Validate Sms Code Send Err. Mobile:{mobile}, Code:{sendResult?.Code}, Message:{sendResult?.Message}");
                }

                return sendResult;
            });
        }

        public bool Validate(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != _options.TemplateIdentityValidation.CodeLength || !ValidationMethods.IsPositiveNumber(code))
            {
                return false;
            }

            string cachedSmsCode = GetSmsCodeFromCache(mobile);

            return string.Equals(code, cachedSmsCode, GlobalSettings.Comparison);
        }

        private void CacheSmsCode(string mobile, string cachedSmsCode, int expireMinutes)
        {
            _cache.SetString(
                        GetCachedKey(mobile),
                        cachedSmsCode,
                        new DistributedCacheEntryOptions() {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expireMinutes)
                        });
        }

        private string GetSmsCodeFromCache(string mobile)
        {
            return _cache.GetString(GetCachedKey(mobile));
        }

        private string GenerateNewSmsCode(int codeLength)
        {
            return SecurityUtil.CreateRandomNumbericString(codeLength);
        }

        private static string GetCachedKey(string mobile)
        {
            return mobile + "_vlc";
        }



    }
}
