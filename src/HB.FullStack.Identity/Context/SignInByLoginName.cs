using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Identity
{
    public class SignInByLoginName : SignInContext, IHasPassword
    {
        public SignInByLoginName(
            string loginName,
            string password,
            string audience,
            bool rememberMe,
            SignInExclusivity exclusivity,
            ClientInfos clientInfos,
            DeviceInfos deviceInfos)
            : base(audience, rememberMe, exclusivity, clientInfos, deviceInfos)
        {
            LoginName = loginName;
            Password = password;
        }

        [LoginName(CanBeNull = false)]
        public string LoginName { get; set; }

        [Password(CanBeNull = false)]
        public string Password { get; set; }

    }
}