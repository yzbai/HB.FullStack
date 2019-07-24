using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Tencent
{
    public class TCapthaOptions : IOptions<TCapthaOptions>
    {
        public const string EndpointName = "Tecent_Captha";

        public TCapthaOptions Value => this;

        public string Endpoint { get; set; }

        public IList<ApiKeySetting> ApiKeySettings { get; private set; } = new List<ApiKeySetting>();
    }

    public class ApiKeySetting
    {
        public string AppId { get; set; }

        public string AppSecretKey { get; set; }
    }
}
