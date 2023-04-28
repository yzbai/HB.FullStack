using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;

namespace HB.FullStack.Common.Shared
{
    public class ClientInfos : ValidatableObject
    {
        [Required]
        public string ClientId { get; set; } = default!;

        [Required]
        public string ClientVersion { get; set; } = default!;

        [Required]
        public string ClientIp { get; set; } = default!;
    }
}