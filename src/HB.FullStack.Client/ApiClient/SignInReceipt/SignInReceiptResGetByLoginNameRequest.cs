using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared;

using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.ApiClient
{
    internal class SignInReceiptResGetByLoginNameRequest : ApiRequest
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


        public SignInReceiptResGetByLoginNameRequest(string loginName, string password, string audience, DeviceInfos deviceInfos) : base(nameof(SignInReceiptRes), ApiMethod.Get, ApiRequestAuth.NONE, CommonApiConditions.ByLoginName)
        {
            LoginName = loginName;
            Password = password;
            Audience = audience;
            DeviceInfos = deviceInfos;
        }
    }
}
