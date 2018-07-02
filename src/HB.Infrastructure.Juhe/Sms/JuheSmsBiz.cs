using HB.Framework.Business;
using HB.Framework.Database;
using HB.Framework.KVStore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using HB.Framework.Common;
using System.Threading.Tasks;
using HB.Framework.EventBus;
using HB.Framework.CommonCompnents.Sms;

namespace HB.Infrastructure.Juhe.Sms
{
    public class JuheSmsBiz : ISmsBiz
    {
        private JuheSmsOptions _options;

        //private Dictionary<SmsMessageType, TemplateSetting> _templateDict;

        public string MobileParameter
        {
            get
            {
                return _options.MobileParameter;
            }
        }

        public string MobileCodeParameter
        {
            get
            {
                return _options.MobileCodeParameter;
            }
        }

        public string ParamSmsMobile => throw new System.NotImplementedException();

        public string ParamSmsIdentityValidationCode => throw new System.NotImplementedException();

        public string ParamSmsMobileValue => throw new System.NotImplementedException();

        public JuheSmsBiz(IDatabase database, IKVStore kvstore, IDistributedCache cache, IEventBus eventBus, IOptions<JuheSmsOptions> options, ILogger<JuheSmsBiz> logger) 
            
        {
            _options = options.Value;
            //_templateDict = new Dictionary<SmsMessageType, TemplateSetting>();

            foreach (TemplateSetting ts in _options.TemplateSettings)
            {
                //_templateDict[ts.Type] = ts;
            }
        }

        //public void SendValidationCodeAsync(string mobile)
        //{
        //    string code = SecurityHelper.CreateRandomNumbericString(_options.CodeLength);


        //    Cache.SetAsync(getCodeKey(mobile), Encoding.UTF8.GetBytes(code), new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = _options.ExpireTimeRange });

        //    //TemplateSetting templateSetting = getTemplate(SmsMessageType.ValidateCode);

        //    //SmsContent smsContent = new SmsContent(SmsMessageType.ValidateCode, mobile);

        //    //smsContent.Parameters.Add("key", _options.AppKey);
        //    //smsContent.Parameters.Add("mobile", mobile);
        //    //smsContent.Parameters.Add("tpl_id", templateSetting.TemplateId.ToString());
        //    //smsContent.Parameters.Add("tpl_value", string.Format(templateSetting.Template, "baiyuzhao", code));

        //    //send(smsContent);
        //}

        //public bool ValidateCode(string mobile, string code)
        //{
        //    if (string.IsNullOrEmpty(code))
        //    {
        //        return false;
        //    }

        //    byte[] cachedByte = Cache.Get(getCodeKey(mobile));

        //    if (cachedByte == null)
        //    {
        //        return false;
        //    }

        //    string cachedCode = Encoding.UTF8.GetString(cachedByte);

        //    Cache.RemoveAsync(getCodeKey(mobile));

        //    return code.Equals(cachedCode);
        //}

        public void SendNotification(string mobile, string friendName, string message)
        {
            //TemplateSetting template = getTemplate(SmsMessageType.Notification);

            //SmsContent sms = new SmsContent(SmsMessageType.Notification, mobile);

            //sms.Parameters.Add("key", _options.AppKey);
            //sms.Parameters.Add("mobile", mobile);
            //sms.Parameters.Add("tpl_id", template.TemplateId.ToString());
            //sms.Parameters.Add("tpl_value", string.Format("#friend#={0}&#message#={1}&#company#={2}", friendName, message, template.Signagure));

            ////TODO: 异步记录到数据库

            //send(sms);
        }

        //private void send(SmsContent content)
        //{
        //    if (!check(content.Mobile))
        //    {
        //        Logger.LogWarning("Mobile {0} has send problem, Message Type {1} ", content.Mobile, content.Type);
        //    }

        //    TemplateSetting templateSetting = getTemplate(content.Type);

        //    string url = QueryHelpers.AddQueryString(templateSetting.ApiBaseUrl, content.Parameters);

        //    HttpClient httpClient = new HttpClient();

        //    httpClient.GetAsync(url).ContinueWith(async t =>
        //    {
        //        HttpResponseMessage responseMsg = t.Result;
        //        httpClient.Dispose();

        //        Logger.LogDebug(await responseMsg.Content.ReadAsStringAsync());

        //        //TODO: 处理返回，日志
        //    });
        //}

        private string getCodeKey(string mobile)
        {
            return ":C:" + mobile;
        }

        //private TemplateSetting getTemplate(SmsMessageType type)
        //{
        //    return _templateDict[type];
        //}

        public bool check(string mobile)
        {
            //1, 同一Session
            //2, 同一Mobile
            //3, 同一IP

            return true;
        }

        public Task<SmsResponseResult> SendIdentityValidationCode(string mobile)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> VerifyIdentityValidateCode(string mobile, string code)
        {
            throw new System.NotImplementedException();
        }

        public Task<SmsResponseResult> SendIdentityValidationCode(string mobile, out string code)
        {
            throw new System.NotImplementedException();
        }
    }
}
