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

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsBiz : ISmsService
    {
        private AliyunSmsOptions _options;
        private IAcsClient _client;
        private ILogger _logger;

        public AliyunSmsBiz(IAcsClientManager acsClientManager, IOptions<AliyunSmsOptions> options, ILogger<AliyunSmsBiz> logger) 
        {
            _options = options.Value;
            _client = acsClientManager.GetAcsClient(_options.ProductName);
            _logger = logger;
        }

        public Task<SendResponse> SendValidationCode(string mobile, out string code)
        {
            code = SecurityHelper.CreateRandomNumbericString(_options.TemplateIdentityValidation.CodeLength);

            SendSmsRequest request = new SendSmsRequest
            {
                AcceptFormat = FormatType.JSON,
                SignName = _options.SignName,
                TemplateCode = _options.TemplateIdentityValidation.TemplateCode,
                PhoneNumbers = mobile,
                TemplateParam = string.Format("{{\"{0}\":\"{1}\", \"{2}\":\"{3}\"}}", 
                    _options.TemplateIdentityValidation.ParamCode, 
                    code, 
                    _options.TemplateIdentityValidation.ParamProduct, 
                    _options.TemplateIdentityValidation.ParamProductValue)
            };

            
            return TaskRetry.Retry(1, () => Task.Run<SendSmsResponse>(() => _client.GetAcsResponse(request)), 
                (response, ex) => {
                    _logger.LogError("Validation Sms Send Err. {0}, {1}, {2}", mobile, _options.TemplateIdentityValidation.ParamProductValue, ex.Message);

                })
                .ContinueWith(t=> {

                    if (t.IsFaulted || t.Result == null)
                    {
                        _logger.LogError("Validation Sms Send Err, Result is empty");
                        return new SendResponse() { Message = "Error.", Succeeded = false };
                    }

                    _logger.LogInformation("Validation Sms Sended. Mobile:{0}, {1}, Content : {2}", mobile, _options.TemplateIdentityValidation.ParamProductValue, 
                        Encoding.UTF8.GetString(t.Result.HttpResponse.Content));

                    return new SendResponse() { Message = t.Result.Message, Succeeded = true };
                });
        }
    }
}
