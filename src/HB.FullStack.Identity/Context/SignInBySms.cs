using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Identity
{
    public class SignInBySms : SignInContext, IBySmsCode
    {
        public SignInBySms(
            string mobile,
            string smsCode,
            bool registerIfNotExists,
            string audience, 
            bool rememberMe, 
            SignInExclusivity exclusivity, 
            ClientInfos clientInfos,
            DeviceInfos deviceInfos) 
            : base(audience, rememberMe, exclusivity, clientInfos, deviceInfos)
        {
            Mobile = mobile;
            SmsCode = smsCode;
            RegisterIfNotExists = registerIfNotExists;
        }

        [Mobile(CanBeNull = false)]
        public string Mobile { get; set; }

        [SmsCode(CanBeNull = false)]
        public string SmsCode { get; set; }

        /// <summary>
        /// 如果User不存在，则注册
        /// </summary>
        public bool RegisterIfNotExists { get; set; }
    }
}