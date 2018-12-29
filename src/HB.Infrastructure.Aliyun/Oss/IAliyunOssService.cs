using System.Threading.Tasks;
using HB.Infrastructure.Aliyun.Sts;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        Task<StsRoleCredential> GetDirectoryReadRoleCredentialAsync(string bucket, string directory, string roleSessionName);
        Task<StsRoleCredential> GetDirectoryWriteRoleCredentialAsync(string bucket, string directory, string roleSessionName);
        Task<StsRoleCredential> GetUserReadRoleCredentialAsync(string bucket, string userGuid);
        Task<StsRoleCredential> GetUserWriteRoleCredentialAsync(string bucket, string userGuid);
    }
}