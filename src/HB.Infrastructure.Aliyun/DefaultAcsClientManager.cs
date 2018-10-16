using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public class DefaultAcsClientManager : IAcsClientManager
    {
        private IDictionary<string, IAcsClient> _clients;

        public DefaultAcsClientManager()
        {
            _clients = new Dictionary<string, IAcsClient>();
        }

        public void AddProfile(string productName, IClientProfile profile)
        {
            if (string.IsNullOrWhiteSpace(productName) || profile == null)
            {
                throw new ArgumentNullException("Aliyun client profile 配置不能为空，或者productname不能为空.");
            }

            if (_clients.ContainsKey(productName))
            {
                throw new ArgumentException("Aliyun client profile 中已经配置了相同的productName:" + productName);
            }

            DefaultAcsClient defaultAcsClient = new DefaultAcsClient(profile);

            _clients.Add(productName, defaultAcsClient);
        }

        public IAcsClient GetAcsClient(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName) || !_clients.ContainsKey(productName))
            {
                return null;
            }

            return _clients[productName];
        }
    }
}
