using System;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Sts
{
    public interface IAliyunStsService
    {
        ///<summary>
        /// 请求阿里云获取临时Oss访问Token，只是干这个. directory没有slash结尾
        ///</summary>        
        /// <exception cref="AliyunException"></exception>
#pragma warning disable CA1716 // Identifiers should not match keywords
        AliyunStsToken? RequestOssStsToken(Guid userId, string bucketName, string directory, bool readOnly);
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}