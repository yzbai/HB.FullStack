using HB.FullStack.Common;
using HB.FullStack.Common.Shared;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Server.Identity
{
    public class RefreshContext : ValidatableObject
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
        
        [ValidatedObject(CanBeNull = false)]        
        public ClientInfos ClientInfos { get; set; }

        [ValidatedObject(CanBeNull = false)]
        public DeviceInfos DeviceInfos { get; set; }

        public RefreshContext(string accessToken, string refreshToken, ClientInfos clientInfos, DeviceInfos deviceInfos)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ClientInfos = clientInfos;
            DeviceInfos = deviceInfos;
        }
    }
}
