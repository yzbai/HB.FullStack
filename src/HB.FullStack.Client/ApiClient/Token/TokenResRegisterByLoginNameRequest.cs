using HB.FullStack.Common.Shared;

using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.ApiClient
{
    internal class TokenResRegisterByLoginNameRequest : ApiRequest
    {
        public TokenResRegisterByLoginNameRequest(string loginName, string password, string audience, DeviceInfos deviceInfos)
            : base(nameof(TokenRes), ApiMethod.Add, ApiRequestAuth.NONE, SharedNames.Conditions.ByLoginName)
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
