using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Server.Identity.Context
{
    public class RegisterContext : ValidatableObject, IHasAudience
    {
        public RegisterContext(string audience, ClientInfos clientInfos, DeviceInfos deviceInfos)
        {
            Audience = audience;
            ClientInfos = clientInfos;
            DeviceInfos = deviceInfos;
        }

        [Required]
        public string Audience { get; set; } = default!;

        [ValidatedObject(CanBeNull = false)]
        public ClientInfos ClientInfos { get; set; }

        [ValidatedObject(CanBeNull = false)]
        public DeviceInfos DeviceInfos { get; set; } = default!;


    }
}
