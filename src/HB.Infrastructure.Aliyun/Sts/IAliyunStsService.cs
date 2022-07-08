using System;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Sts
{
    public interface IAliyunStsService
    {
        ///<summary>
        /// 请求阿里云获取临时Oss访问Token，只是干这个. directory没有slash结尾
        ///</summary>        
        
        StsToken? RequestOssStsToken(Guid userId, string bucketName, string directory, bool readOnly, double expirySeconds);
    }
}