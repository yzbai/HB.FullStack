#nullable enable

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Tencent
{
    public class TCapthaOptions : IOptions<TCapthaOptions>
    {
        public const string ENDPOINT_NAME = "Tecent_Captha";

        public TCapthaOptions Value
        {
            get
            {
                return this;
            }
        }

        public string Endpoint { get; set; } = default!;

        public IList<ApiKeySetting> ApiKeySettings { get; private set; } = new List<ApiKeySetting>();
    }

    public class ApiKeySetting
    {
        public string AppId { get; set; } = null!;

        public string AppSecretKey { get; set; } = null!;

        public ApiKeySetting() { }

        public ApiKeySetting(string appId, string appSecretKey)
        {
            AppId = appId;
            AppSecretKey = appSecretKey;
        }
    }
}
