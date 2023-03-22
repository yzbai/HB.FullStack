using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class SignInReceiptResGetBySmsRequest : ApiRequest
    {
        [RequestQuery]
        [Mobile(CanBeNull = false)]
        public string Mobile { get; set; } = null!;

        [RequestQuery]
        [SmsCode(CanBeNull = false)]
        public string SmsCode { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string SignToWhere { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string ClientId { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string ClientVersion { get; set; } = null!;

        [RequestQuery]
        [Required]
        public DeviceInfos DeviceInfos { get; set; } = null!;

        public SignInReceiptResGetBySmsRequest(string mobile, string smsCode, string signToWhere, string clientId, string clientVersion, DeviceInfos deviceInfos)
            : base(nameof(SignInReceiptRes), ApiMethod.Get, ApiRequestAuth.NONE, "BySms")
        {
            Mobile = mobile;
            SmsCode = smsCode;
            SignToWhere = signToWhere;
            ClientId = clientId;
            ClientVersion = clientVersion;
            DeviceInfos = deviceInfos;
        }
    }
}
