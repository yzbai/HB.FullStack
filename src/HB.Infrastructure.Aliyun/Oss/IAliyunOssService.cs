using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        /// <exception cref="AliyunOssException"></exception>
        AliyunStsToken GetUserDirectoryToken(string bucket, string userGuid, bool isRead);

        /// <exception cref="AliyunOssException"></exception>
        AliyunStsToken GetDirectoryToken(string bucket, string directory, string roleSessionName, bool isRead);

        string GetOssEndpoint(string bucket);

        string GetRegionId(string bucket);

        string UserBucketName { get; }

        string PublicBucketName { get; }
    }
}