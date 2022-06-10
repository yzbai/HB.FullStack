﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.CommonTests.ApiClient
{
    public class PreferenceProviderStub : IPreferenceProvider
    {
        public Guid? UserId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public string DeviceId { get; } = Guid.NewGuid().ToString();

        public string DeviceVersion { get; } = "1.0";

        public DeviceInfos DeviceInfos { get; } = new DeviceInfos
        {
            Name = "Stub_Name",
            Model = "Stub_Model",
            OSVersion = "Stub_OSVersion",
            Platform = "Android",
            Type = "Stub_Type"
        };

        public bool IsLogined()
        {
            return false;
        }

        public void OnTokenFetched(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public void OnTokenRefreshFailed()
        {

        }
    }
}
