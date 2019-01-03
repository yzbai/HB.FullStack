using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;

namespace HB.Infrastructure.Aliyun
{
    public interface IAcsClientManager
    {
        void AddClient(AliyunAccessSetting accessSetting, IClientProfile profile);
        IAcsClient GetAcsClient(string productName);
        AliyunAccessSetting GetAcessSetting(string productName);
    }
}