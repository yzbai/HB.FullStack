using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity.Context;

namespace HB.FullStack.Server.Identity
{
    public class SignInByEmail : SignInContext, IHasPassword
    {
        public SignInByEmail(
            string email,
            string password,
            string audience,
            bool rememberMe,
            SignInExclusivity exclusivity,
            ClientInfos clientInfos,
            DeviceInfos deviceInfos)
            : base(audience, rememberMe, exclusivity, clientInfos, deviceInfos)
        {
            Email = email;
            Password = password;
        }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Password(CanBeNull = false)]
        public string Password { get; set; }
    }
}