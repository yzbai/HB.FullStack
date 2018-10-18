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

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsBiz : ISmsService
    {
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        private AliyunSmsOptions _options;
        private IAcsClient _client;
        private readonly ILogger _logger;

        public AliyunSmsBiz(IAcsClientManager acsClientManager, IOptions<AliyunSmsOptions> options, ILogger<AliyunSmsBiz> logger) 
        {
            _options = options.Value;
            _client = acsClientManager.GetAcsClient(_options.ProductName);
            _logger = logger;
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
                TemplateParam = string.Format(_culture, "{{\"{0}\":\"{1}\", \"{2}\":\"{3}\"}}", 
                    _options.TemplateIdentityValidation.ParamCode, 
                    code, 
                    _options.TemplateIdentityValidation.ParamProduct, 
                    _options.TemplateIdentityValidation.ParamProductValue)
            };

            return PolicyManager.Default(_logger).ExecuteAsync(async ()=> {
                Task<SendSmsResponse> task = new Task<SendSmsResponse>(() => _client.GetAcsResponse(request));
                task.Start(TaskScheduler.Default);

                SendSmsResponse result = await task.ConfigureAwait(false);

                return SendResultTransformer.Transform(result);
            });
        }
    }
}
