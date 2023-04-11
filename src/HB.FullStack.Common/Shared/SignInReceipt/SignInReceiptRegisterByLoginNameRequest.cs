using System;

using HB.FullStack.Common.Shared.Attributes;

namespace HB.FullStack.Common.Shared.SignInReceipt
{
    internal class SignInReceiptRegisterByLoginNameRequest : ApiRequest
    {
        public SignInReceiptRegisterByLoginNameRequest(string loginName, string audience, string password, DeviceInfos deviceInfos)
            : base(nameof(SignInReceiptRes), ApiMethod.Add, ApiRequestAuth.NONE, "RegisterByLoginName")
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
