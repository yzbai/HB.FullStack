using HB.FullStack.Common.Shared.Attributes;

namespace HB.FullStack.Common.Shared.Sms
{
    internal class SmsValidationCodeResGetByMobileRequest : ApiRequest
    {
        public SmsValidationCodeResGetByMobileRequest(string mobile) : base(nameof(SmsValidationCodeRes), ApiMethod.Get, ApiRequestAuth.NONE, CommonApiConditions.ByMobile)
        {
            Mobile = mobile;
        }

        [RequestQuery]
        public string Mobile { get; }
    }
}