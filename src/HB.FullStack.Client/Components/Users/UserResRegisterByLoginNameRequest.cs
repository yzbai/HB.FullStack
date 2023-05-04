using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;



namespace HB.FullStack.Client.Components
{
    internal class UserResRegisterByLoginNameRequest : ApiRequest
    {
        public UserResRegisterByLoginNameRequest(string loginName, string password, string audience, DeviceInfos deviceInfos)
            : base("UserRes", ApiMethod.Add, ApiRequestAuth.NONE, SharedNames.Conditions.ByLoginName)
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
