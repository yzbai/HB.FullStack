using HB.Framework.Common.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.Component.Authorization.Abstractions
{
    public class RefreshContext : CommonEntity
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        public string ClientId { get; set; }
        public string ClientType { get; set; }
        public string ClientVersion { get; set; }
        public string ClientAddress { get; set; }
    }
}
