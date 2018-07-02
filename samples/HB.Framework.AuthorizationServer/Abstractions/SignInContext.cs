using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using HB.Framework.Common.Entity;

namespace HB.Framework.AuthorizationServer.Abstractions
{
    public enum SignInType
    {
        BySms,
        ByMobileAndPassword,
        ByUserNameAndPassword
    }

    public enum ClientType
    {
        None = 0,
        Android = 1,
        Iphone = 2,
        Web = 3,
        Postman = 4
    }

    public class SignInContext : CommonEntity
    {
        public HttpContext HttpContext { get; set; }

        public SignInType SignInType { get; set; }

        [UserName]
        public string UserName { get; set; }

        [Password]
        public string Password { get; set; }

        [Mobile]
        public string Mobile { get; set; }
        public bool RememberMe { get; set; }

        public string ClientId { get; set; }
        public ClientType ClientType { get; set; }
        public string ClientVersion { get; set; }
        public string ClientAddress { get; set; }
        public string ClientIp { get; set; }

        public string SignToWhere { get; set; }
    }
}
