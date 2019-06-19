using System.Threading.Tasks;
using HB.Infrastructure.Aliyun.Sts;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        Task<StsRoleCredential> GetDirectoryRoleCredentialAsync(string bucket, string directory, string roleSessionName, bool isRead);

        Task<StsRoleCredential> GetUserRoleCredentialAsync(string bucket, string userGuid, bool isRead);

        int GetExpireSeconds(string bucket);

        string GetEndpoint();

        string GetRegion();
    }
}