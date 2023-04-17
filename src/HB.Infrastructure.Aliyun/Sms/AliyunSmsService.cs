using System;
using System.Text.Json;
using System.Threading.Tasks;

using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Http;

using AsyncAwaitBestPractices;

using HB.FullStack.Cache;
using HB.FullStack.Common.Validate;
using HB.FullStack.Server.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ClientException = Aliyun.Acs.Core.Exceptions.ClientException;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsService : ISmsService
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

            AliyunUtil.AddEndpoint(AliyunProductNames.SMS, _options.RegionId, _options.Endpoint);
            _client = AliyunUtil.CreateAcsClient(_options.RegionId, _options.AccessKeyId, _options.AccessKeySecret);
        }

        public async Task SendValidationCodeAsync(string mobile/*, out string smsCode*/)
        {
            string smsCode = GenerateNewSmsCode(_options.TemplateIdentityValidation.CodeLength);

            string templateParam = string.Format(Globals.Culture, "{{\"{0}\":\"{1}\", \"{2}\":\"{3}\"}}",
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
                CommonResponse response = await PolicyManager
                    .SendSmsRetryPolicy(_logger)
                    .ExecuteAsync(async () =>
                    {
                        return await _client.GetCommonResponseAsync(request).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                SendResult? sendResult = SerializeUtil.FromJson<SendResult>(response.Data);

                if (sendResult != null && sendResult.IsSuccessful())
                {
                    SetSmsCodeToCacheAsync(mobile, smsCode, _options.TemplateIdentityValidation.ExpireMinutes)
                        .SafeFireAndForget(ex =>
                        {
                            _logger.LogCritical(ex, "Aliyun Sms 服务在使用缓存时出错，有可能缓存不可用。");
                        });
                }
                else
                {
                    string errorMessage = $"Validate Sms Code Send Err. Mobile:{mobile}, Code:{sendResult?.Code}, Message:{sendResult?.Message}";
                    throw AliyunExceptions.SmsSendError(mobile: mobile, code: sendResult?.Code, message: sendResult?.Message);
                }
            }
            catch (CacheException ex)
            {
                throw AliyunExceptions.SmsCacheError("", ex);
            }
            catch (AliyunException ex)
            {
                throw AliyunExceptions.SmsServerError("", ex);
            }
            catch (JsonException ex)
            {
                throw AliyunExceptions.SmsFormatError("阿里云短信服务，格式返回错误", ex);
            }
            catch (ClientException ex)
            {
                throw AliyunExceptions.SmsClientError("AliyunSmsServiceDownErrorMessage", ex);
            }
        }

#if DEBUG
        public async Task SendValidationCodeAsync(string mobile, string smsCode, int expiryMinutes)
        {
            try
            {
                await SetSmsCodeToCacheAsync(mobile, smsCode, expiryMinutes).ConfigureAwait(false);

            }
            catch (CacheException ex)
            {
                throw AliyunExceptions.SmsCacheError("在设置SmsCode缓存时出错，请查看缓存可用性!", ex);
            }
        }

#endif

        public async Task<bool> ValidateAsync(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != _options.TemplateIdentityValidation.CodeLength || !ValidationMethods.IsAllNumber(code))
            {
                return false;
            }

            try
            {
                string? cachedSmsCode = await GetSmsCodeFromCacheAsync(mobile).ConfigureAwait(false);

                return string.Equals(code, cachedSmsCode, Globals.Comparison);
            }
            catch (CacheException ex)
            {
                throw AliyunExceptions.SmsCacheError("", ex);
            }
        }

        private Task SetSmsCodeToCacheAsync(string mobile, string cachedSmsCode, int expireMinutes)
        {
            return _cache.SetStringAsync(
                        GetCachedKey(mobile),
                        cachedSmsCode,
                        TimeUtil.Timestamp,
                        new DistributedCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expireMinutes)
                        });
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

        private class SendResult
        {
            public string? Code { get; set; }

            public string? Message { get; set; }

            public bool IsSuccessful()
            {
                return "OK".Equals(Code, Globals.ComparisonIgnoreCase);
            }
        }
    }
}