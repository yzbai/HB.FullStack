using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Aliyun.Acs.Core;
using System.Threading.Tasks;
using Aliyun.Acs.Core.Http;
using System;
using Microsoft.Extensions.Caching.Distributed;
using HB.FullStack.Common.Validate;
using System.Text.Json;
using ClientException = Aliyun.Acs.Core.Exceptions.ClientException;
using HB.FullStack.Common.Server;
using HB.FullStack.Cache;

namespace HB.Infrastructure.Aliyun.Sms
{
    internal class AliyunSmsService : ISmsServerService
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

        //TODO: 等待阿里云增加异步方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        
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
                    throw AliyunExceptions.SmsSendError(mobile: mobile, code: sendResult?.Code, message:sendResult?.Message);
                }
            }
            catch(CacheException ex)
            {
                throw AliyunExceptions.SmsCacheError("", ex);
            }
            catch(AliyunException ex)
            {
                throw AliyunExceptions.SmsServerError("", ex);
            }
            catch (JsonException ex)
            {
                throw AliyunExceptions.SmsFormatError( "阿里云短信服务，格式返回错误", ex);
            }
            catch (ClientException ex)
            {
                throw AliyunExceptions.SmsClientError("AliyunSmsServiceDownErrorMessage", ex);
            }
        }

#if DEBUG
        /// <summary>
        /// SendValidationCode
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smsCode"></param>
        /// <param name="expiryMinutes"></param>
        
        public void SendValidationCode(string mobile, string smsCode, int expiryMinutes)
        {
            try
            {
                SetSmsCodeToCache(mobile, smsCode, expiryMinutes);
            }
            catch (CacheException ex)
            {
                throw AliyunExceptions.SmsCacheError( "", ex);
            }
        }
#endif

        /// <summary>
        /// ValidateAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        
        public async Task<bool> ValidateAsync(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != _options.TemplateIdentityValidation.CodeLength || !ValidationMethods.IsAllNumber(code))
            {
                return false;
            }

            try
            {
                string? cachedSmsCode = await GetSmsCodeFromCacheAsync(mobile).ConfigureAwait(false);

                return string.Equals(code, cachedSmsCode, GlobalSettings.Comparison);
            }
            catch (CacheException ex)
            {
                throw AliyunExceptions.SmsCacheError( "", ex);
            }
        }

        /// <summary>
        /// SetSmsCodeToCache
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="cachedSmsCode"></param>
        /// <param name="expireMinutes"></param>
        
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

        /// <summary>
        /// GetSmsCodeFromCacheAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        
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
