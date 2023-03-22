using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Context
{
    public class RegisterByLoginName : RegisterContext, IHasPassword
    {
        public RegisterByLoginName(string loginName, string password, string audience, ClientInfos clientInfos, DeviceInfos deviceInfos) : base(audience, clientInfos, deviceInfos)
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
