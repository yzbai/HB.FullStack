using System;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        /// <exception cref="AliyunException"></exception>
        AliyunStsToken GetUserDirectoryToken(string bucket, string userGuid, bool isRead);

        /// <exception cref="AliyunException"></exception>
        AliyunStsToken GetDirectoryToken(string bucket, string directory, string roleSessionName, bool isRead);

        /// <exception cref="AliyunException"></exception>
        string GetOssEndpoint(string bucket);

        /// <exception cref="AliyunException"></exception>
        string GetRegionId(string bucket);

        string UserBucketName { get; }

        string PublicBucketName { get; }
    }
}