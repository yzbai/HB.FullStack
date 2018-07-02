using System;
using Microsoft.Extensions.Options;

namespace HB.Framework.Http
{
    public class SiteOptions : IOptions<SiteOptions>
    {
        public SiteOptions Value { get { return this; } }

        #region Auth

        public string ApplicationDataProtectionDiscriminator { get; set; }

        #endregion



    }
}