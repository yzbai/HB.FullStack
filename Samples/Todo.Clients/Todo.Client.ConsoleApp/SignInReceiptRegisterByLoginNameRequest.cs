using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace Todo.Client.ConsoleApp
{
    internal class SignInReceiptRegisterByLoginNameRequest : ApiRequest
    {
        public SignInReceiptRegisterByLoginNameRequest(string loginName, string audience, string clientId, string clientVersion, DeviceInfos deviceInfos) 
            : base(nameof(SignInReceiptRes), ApiMethod.Add, ApiRequestAuth.NONE, "RegisterByLoginName")
        {
            LoginName = loginName;
            Audience = audience;
            ClientId = clientId;
            ClientVersion = clientVersion;
            DeviceInfos = deviceInfos;
        }

        [RequestQuery]
        public string LoginName { get; set; }
        [RequestQuery]
        public string Audience { get; set; }
        [RequestQuery]
        public string ClientId { get; set; }
        [RequestQuery]
        public string ClientVersion { get; set; }
        [RequestQuery]
        public DeviceInfos DeviceInfos { get; set; }
    }
}
