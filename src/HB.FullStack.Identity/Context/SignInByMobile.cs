using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity.Context;

namespace HB.FullStack.Server.Identity
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