using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;

namespace HB.Infrastructure.Aliyun
{
    public interface IAcsClientManager
    {
        void AddProfile(string productName, IClientProfile profile);
        IAcsClient GetAcsClient(string productName);
    }
}