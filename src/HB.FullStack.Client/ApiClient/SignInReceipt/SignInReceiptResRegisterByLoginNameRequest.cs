using HB.FullStack.Common.Shared;

using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.ApiClient
{
    internal class SignInReceiptResRegisterByLoginNameRequest : ApiRequest
    {
        public SignInReceiptResRegisterByLoginNameRequest(string loginName, string password, string audience, DeviceInfos deviceInfos)
            : base(nameof(SignInReceiptRes), ApiMethod.Add, ApiRequestAuth.NONE, CommonApiConditions.ByLoginName)
        {
            LoginName = loginName;
            Audience = audience;
            Password = password;
            DeviceInfos = deviceInfos;
        }

        [RequestQuery]
        public string LoginName { get; set; }
        [RequestQuery]
        public string Audience { get; set; }

        [RequestQuery]
        public string Password { get; set; }


        [RequestQuery]
        public DeviceInfos DeviceInfos { get; set; }
    }
}
