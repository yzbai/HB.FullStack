using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Oss
{
    public interface IAliyunOssService
    {
        /// <exception cref="Aliyun.Acs.Core.Exceptions.ServerException"></exception> 
        /// <exception cref="Aliyun.Acs.Core.Exceptions.ClientException"></exception> 
        Task<AliyunStsToken> GetUserDirectoryTokenAsync(string bucket, string userGuid, bool isRead);

        /// <exception cref="Aliyun.Acs.Core.Exceptions.ServerException"></exception> 
        /// <exception cref="Aliyun.Acs.Core.Exceptions.ClientException"></exception> 
        Task<AliyunStsToken> GetDirectoryTokenAsync(string bucket, string directory, string roleSessionName, bool isRead);

        string GetOssEndpoint(string bucket);

        string GetRegionId(string bucket);

        string UserBucketName { get; }

        string PublicBucketName { get; }
    }
}