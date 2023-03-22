using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity
{
    public class SignInByMobile : SignInContext, IHasPassword
    {
        public SignInByMobile(
            string mobile,
            string password,
            string audience,
            bool rememberMe,
            SignInExclusivity exclusivity,
            ClientInfos clientInfos,
            DeviceInfos deviceInfos)
            : base(audience, rememberMe, exclusivity, clientInfos, deviceInfos)
        {
            Mobile = mobile;
            Password = password;
        }

        [Mobile(CanBeNull = false)]
        public string Mobile { get; set; }

        [Password(CanBeNull = false)]
        public string Password { get; set; }
    }
}