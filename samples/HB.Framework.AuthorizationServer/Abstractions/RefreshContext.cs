using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.Framework.AuthorizationServer.Abstractions
{
    public class RefreshContext
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        public string ClientId { get; set; }
        public ClientType ClientType { get; set; }
        public string ClientVersion { get; set; }
        public string ClientAddress { get; set; }
    }
}
