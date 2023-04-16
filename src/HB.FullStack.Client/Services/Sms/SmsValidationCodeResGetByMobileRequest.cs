using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;

using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.Services.Sms
{
    internal class SmsValidationCodeResGetByMobileRequest : ApiRequest
    {
        public SmsValidationCodeResGetByMobileRequest(string mobile) 
            : base(nameof(SmsValidationCodeRes), ApiMethod.Get, ApiRequestAuth.NONE, CommonApiConditions.ByMobile)
        {
            Mobile = mobile;
        }

        [RequestQuery]
        public string Mobile { get; }
    }
}