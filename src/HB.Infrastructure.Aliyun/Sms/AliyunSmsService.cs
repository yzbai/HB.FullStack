using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Aliyun.Acs.Core;
using HB.Framework.Common;
using System.Threading.Tasks;
using Aliyun.Acs.Core.Http;
using HB.Compnent.Resource.Sms;
using Aliyun.Acs.Dysmsapi.Model.V20170525;
using HB.Component.Resource.Sms.Entity;
using System.Globalization;
using Polly;
using Aliyun.Acs.Core.Exceptions;
using HB.Infrastructure.Aliyun.Sms.Transform;
using System;
using Microsoft.Extensions.Caching.Distributed;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsBiz : ISmsService
    {
        private AliyunSmsOptions _options;
        private IAcsClient _client;
        private readonly ILogger _logger;
        private IDistributedCache _cache;

        public AliyunSmsBiz(IAcsClientManager acsClientManager, IOptions<AliyunSmsOptions> options, ILogger<AliyunSmsBiz> logger, IDistributedCache cache) 
        {
            _options = options.Value;
            _client = acsClientManager.GetAcsClient(_options.ProductName);
            _logger = logger;
            _cache = cache;
        }

        public Task<SendResult> SendValidationCode(string mobile, out string code)
        {
            code = SecurityHelper.CreateRandomNumbericString(_options.TemplateIdentityValidation.CodeLength);

            SendSmsRequest request = new SendSmsRequest
            {
                AcceptFormat = FormatType.JSON,
                SignName = _options.SignName,
                TemplateCode = _options.TemplateIdentityValidation.TemplateCode,
                PhoneNumbers = mobile,
                TemplateParam = string.Format(GlobalSettings.Culture, "{{\"{0}\":\"{1}\", \"{2}\":\"{3}\"}}", 
                    _options.TemplateIdentityValidation.ParamCode, 
                    code, 
                    _options.TemplateIdentityValidation.ParamProduct, 
                    _options.TemplateIdentityValidation.ParamProductValue)
            };

            string cachedValue = code;

            return PolicyManager.Default(_logger).ExecuteAsync(async ()=> {
                Task<SendSmsResponse> task = new Task<SendSmsResponse>(() => _client.GetAcsResponse(request));
                task.Start(TaskScheduler.Default);

                SendSmsResponse result = await task.ConfigureAwait(false);

                if (result.Code == "OK")
                {
                    _cache.SetString(
                        getCachedKey(mobile), 
                        cachedValue, 
                        new DistributedCacheEntryOptions() { 
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.TemplateIdentityValidation.ExpireMinutes)
                        });
                }

                return SendResultTransformer.Transform(result);
            });
        }

        public bool Validate(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            string storedCode = _cache.GetString(getCachedKey(mobile));

            return string.Equals(code, storedCode, GlobalSettings.Comparison);
        }

        public string getCachedKey(string mobile)
        {
            return mobile + "_vlc";
        }
    }
}
