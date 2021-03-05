using System;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Sts
{
    public interface IAliyunStsService
    {
        ///<summary>
        /// 请求阿里云获取临时Oss访问Token，只是干这个
        ///</summary>        
        /// <exception cref="AliyunException"></exception>
        AliyunStsToken? RequestOssStsToken(long userId, string bucketName, string directory, bool readOnly);
    }
}