using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        /// <exception cref="Aliyun.AliyunException"></exception>
        AliyunStsToken GetUserDirectoryToken(string bucket, string userGuid, bool isRead);

        /// <exception cref="Aliyun.AliyunException"></exception>
        AliyunStsToken GetDirectoryToken(string bucket, string directory, string roleSessionName, bool isRead);

        /// <exception cref="Aliyun.AliyunException"></exception>
        string GetOssEndpoint(string bucket);

        /// <exception cref="Aliyun.AliyunException"></exception>
        string GetRegionId(string bucket);

        string UserBucketName { get; }

        string PublicBucketName { get; }
    }
}