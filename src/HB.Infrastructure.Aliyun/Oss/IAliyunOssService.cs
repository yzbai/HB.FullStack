using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        /// <exception cref="HB.Infrastructure.Aliyun.Oss.AliyunOssException"></exception>
        AliyunStsToken GetUserDirectoryToken(string bucket, string userGuid, bool isRead);

        /// <exception cref="HB.Infrastructure.Aliyun.Oss.AliyunOssException"></exception>
        AliyunStsToken GetDirectoryToken(string bucket, string directory, string roleSessionName, bool isRead);

        /// <exception cref="HB.Infrastructure.Aliyun.Oss.AliyunOssException"></exception>
        string GetOssEndpoint(string bucket);

        /// <exception cref="HB.Infrastructure.Aliyun.Oss.AliyunOssException"></exception>
        string GetRegionId(string bucket);

        string UserBucketName { get; }

        string PublicBucketName { get; }
    }
}