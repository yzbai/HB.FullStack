﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Aliyun.Acs.Core;
using System.Threading.Tasks;
using Aliyun.Acs.Core.Http;
using System;
using Microsoft.Extensions.Caching.Distributed;
using HB.FullStack.Common.Validate;
using System.Text.Json;
using HB.Infrastructure.Aliyun.Properties;
using ClientException = Aliyun.Acs.Core.Exceptions.ClientException;
using HB.FullStack.Common.Server;
using HB.FullStack.Cache;

namespace HB.Infrastructure.Aliyun.Sms
{
    internal class AliyunSmsService : ISmsService
    {
        private readonly AliyunSmsOptions _options;
        private readonly IAcsClient _client;
        private readonly ILogger _logger;
        private readonly ICache _cache;

        public AliyunSmsService(IOptions<AliyunSmsOptions> options, ILogger<AliyunSmsService> logger, ICache cache)
        {
            _options = options.Value;
            _logger = logger;
            _cache = cache;

            AliyunUtil.AddEndpoint(ProductNames.SMS, _options.RegionId, _options.Endpoint);
            _client = AliyunUtil.CreateAcsClient(_options.RegionId, _options.AccessKeyId, _options.AccessKeySecret);

        }

        //TODO: 等待阿里云增加异步方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Aliyun.Sms.AliyunSmsException"></exception>
        public void SendValidationCode(string mobile/*, out string smsCode*/)
        {
            string smsCode = GenerateNewSmsCode(_options.TemplateIdentityValidation.CodeLength);

            string templateParam = string.Format(GlobalSettings.Culture, "{{\"{0}\":\"{1}\", \"{2}\":\"{3}\"}}",
                    _options.TemplateIdentityValidation.ParamCode,
                    smsCode,
                    _options.TemplateIdentityValidation.ParamProduct,
                    _options.TemplateIdentityValidation.ParamProductValue);

            CommonRequest request = new CommonRequest
            {
                Method = MethodType.POST,
                Domain = "dysmsapi.aliyuncs.com",
                Version = "2017-05-25",
                Action = "SendSms"
            };

            request.AddQueryParameters("PhoneNumbers", mobile);
            request.AddQueryParameters("SignName", _options.SignName);
            request.AddQueryParameters("TemplateCode", _options.TemplateIdentityValidation.TemplateCode);
            request.AddQueryParameters("TemplateParam", templateParam);

            try
            {
                CommonResponse response = PolicyManager.Default(_logger).Execute(() => { return _client.GetCommonResponse(request); });

                SendResult? sendResult = SerializeUtil.FromJson<SendResult>(response.Data);

                if (sendResult != null && sendResult.IsSuccessful())
                {
                    SetSmsCodeToCache(mobile, smsCode, _options.TemplateIdentityValidation.ExpireMinutes);
                }
                else
                {
                    string errorMessage = $"Validate Sms Code Send Err. Mobile:{mobile}, Code:{sendResult?.Code}, Message:{sendResult?.Message}";
                    throw new AliyunSmsException(errorMessage);
                }
            }
            catch (JsonException ex)
            {
                throw new AliyunSmsException(Resources.AliyunSmsResponseFormatErrorMessage, ex);
            }
            catch (ClientException ex)
            {
                throw new AliyunSmsException(Resources.AliyunSmsServiceDownErrorMessage, ex);
            }
        }

#if DEBUG
        public void SendValidationCode(string mobile, string smsCode, int expiryMinutes)
        {
            SetSmsCodeToCache(mobile, smsCode, expiryMinutes);
        }
#endif

        public async Task<bool> ValidateAsync(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != _options.TemplateIdentityValidation.CodeLength || !ValidationMethods.IsPositiveNumber(code))
            {
                return false;
            }

            string? cachedSmsCode = await GetSmsCodeFromCacheAsync(mobile).ConfigureAwait(false);

            return string.Equals(code, cachedSmsCode, GlobalSettings.Comparison);
        }

        private void SetSmsCodeToCache(string mobile, string cachedSmsCode, int expireMinutes)
        {
            _cache.SetStringAsync(
                        GetCachedKey(mobile),
                        cachedSmsCode,
                        TimeUtil.UtcNowTicks,
                        new DistributedCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expireMinutes)
                        }).Fire();
        }

        private Task<string?> GetSmsCodeFromCacheAsync(string mobile)
        {
            return _cache.GetStringAsync(GetCachedKey(mobile));
        }

        private static string GenerateNewSmsCode(int codeLength)
        {
            return SecurityUtil.CreateRandomNumbericString(codeLength);
        }

        private static string GetCachedKey(string mobile)
        {
            return mobile + "_vlc";
        }

        class SendResult
        {
            public string? Code { get; set; }

            public string? Message { get; set; }

            public bool IsSuccessful()
            {
                return "OK".Equals(Code, GlobalSettings.ComparisonIgnoreCase);
            }
        }

    }
}
