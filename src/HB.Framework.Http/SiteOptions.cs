using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Http
{
    public class SiteOptions : IOptions<SiteOptions>
    {
        public SiteOptions Value {
            get {
                return this;
            }
        }

        public int PublicResourceTokenExpireSeconds { get; set; } = 60;
    }
}
