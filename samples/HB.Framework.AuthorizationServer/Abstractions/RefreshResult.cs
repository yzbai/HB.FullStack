using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.AuthorizationServer.Abstractions
{
    public class RefreshResult
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public bool Succeed { get { return (!string.IsNullOrEmpty(AccessToken)) && (!string.IsNullOrEmpty(RefreshToken)); } }
    }
}
