using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using HB.Framework.Common.Entity;

namespace HB.Component.Authorization.Abstractions
{

    public class SignInContext : CommonEntity
    {
        //public HttpContext HttpContext { get; set; }

        public SignInType SignInType { get; set; }

        [Required]
        public string UserType { get; set; }

        [UserName]
        public string UserName { get; set; }

        [Password]
        public string Password { get; set; }

        [Mobile]
        public string Mobile { get; set; }
        public bool RememberMe { get; set; }

        public string ClientId { get; set; }
        public string ClientType { get; set; }
        public string ClientVersion { get; set; }
        public string ClientAddress { get; set; }
        public string ClientIp { get; set; }

        public string SignToWhere { get; set; }
    }
}
