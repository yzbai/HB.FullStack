using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared.Attributes;

namespace HB.FullStack.Common.Shared.SignInReceipt
{
    internal class SignInReceiptResGetBySmsRequest : ApiRequest
    {
        [RequestQuery]
        [Mobile(CanBeNull = false)]
        public string Mobile { get; set; } = null!;

        [RequestQuery]
        [SmsCode(CanBeNull = false)]
        public string SmsCode { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string Audience { get; set; } = null!;

        [RequestQuery]
        [Required]
        public DeviceInfos DeviceInfos { get; set; } = null!;

        public SignInReceiptResGetBySmsRequest(
            string mobile, string smsCode, string signToWhere, DeviceInfos deviceInfos)
            : base(nameof(SignInReceiptRes), ApiMethod.Get, ApiRequestAuth.NONE, CommonApiConditions.BySms)
        {
            Mobile = mobile;
            SmsCode = smsCode;
            Audience = signToWhere;
            DeviceInfos = deviceInfos;
        }
    }
}
