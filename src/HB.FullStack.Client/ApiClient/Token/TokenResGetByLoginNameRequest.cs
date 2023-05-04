using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared;



namespace HB.FullStack.Client.ApiClient
{
    internal class TokenResGetByLoginNameRequest : ApiRequest
    {
        [RequestQuery]
        [LoginName(CanBeNull = false)]
        public string LoginName { get; set; }

        [RequestQuery]
        [Password(CanBeNull = false)]
        public string Password { get; set; }

        [RequestQuery]
        [Required]
        public string Audience { get; set; }

        [RequestQuery]
        [ValidatedObject(CanBeNull =false)]
        public DeviceInfos DeviceInfos { get; set; }


        public TokenResGetByLoginNameRequest(
            string loginName, 
            string password, 
            string audience, 
            DeviceInfos deviceInfos) : base(nameof(TokenRes), ApiMethod.Get, ApiRequestAuth.NONE, SharedNames.Conditions.ByLoginName)
        {
            LoginName = loginName;
            Password = password;
            Audience = audience;
            DeviceInfos = deviceInfos;
        }
    }
}
