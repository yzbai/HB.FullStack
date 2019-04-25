using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public class DefaultAcsClientManager : IAcsClientManager
    {
        private readonly IDictionary<string, IAcsClient> _clients;
        private readonly IDictionary<string, AliyunAccessSetting> _accessSettings;

        public DefaultAcsClientManager()
        {
            _clients = new Dictionary<string, IAcsClient>();
            _accessSettings = new Dictionary<string, AliyunAccessSetting>();
        }

        public void AddClient(AliyunAccessSetting accessSetting, IClientProfile profile)
        {
            accessSetting.ThrowIfNull(nameof(accessSetting));

            if (string.IsNullOrWhiteSpace(accessSetting.ProductName) || profile == null)
            {
                throw new Exception("Aliyun client profile 配置不能为空，或者productname不能为空.");
            }

            if (_clients.ContainsKey(accessSetting.ProductName) || _accessSettings.ContainsKey(accessSetting.ProductName))
            {
                throw new ArgumentException("Aliyun client profile 中已经配置了相同的productName:" + accessSetting.ProductName);
            }

            DefaultAcsClient defaultAcsClient = new DefaultAcsClient(profile);

            _clients.Add(accessSetting.ProductName, defaultAcsClient);

            _accessSettings.Add(accessSetting.ProductName, accessSetting);
        }

        public IAcsClient GetAcsClient(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName) || !_clients.ContainsKey(productName))
            {
                return null;
            }

            return _clients[productName];
        }

        public AliyunAccessSetting GetAcessSetting(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName) || !_accessSettings.ContainsKey(productName))
            {
                return null;
            }

            return _accessSettings[productName];
        }
    }
}
