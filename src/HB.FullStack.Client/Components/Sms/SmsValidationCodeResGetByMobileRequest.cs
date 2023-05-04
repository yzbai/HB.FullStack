using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;



namespace HB.FullStack.Client.Components.Sms
{
    internal class SmsValidationCodeResGetByMobileRequest : ApiRequest
    {
        public SmsValidationCodeResGetByMobileRequest(string mobile) 
            : base(nameof(SmsValidationCodeRes), ApiMethod.Get, ApiRequestAuth.NONE, SharedNames.Conditions.ByMobile)
        {
            Mobile = mobile;
        }

        [RequestQuery]
        public string Mobile { get; }
    }
}