using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        Task<AliyunStsToken> GetUserDirectoryTokenAsync(string bucket, string userGuid, bool isRead);

        Task<AliyunStsToken> GetDirectoryTokenAsync(string bucket, string directory, string roleSessionName, bool isRead);

        string GetOssEndpoint(string bucket);

        string GetRegionId(string bucket);

        string UserBucketName { get; }

        string PublicBucketName { get; }
    }
}