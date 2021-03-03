using System;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Sts
{
    public interface IAliyunStsService
    {
        /// <exception cref="AliyunException"></exception>
        AliyunStsToken? GetStsToken(string resource, long userId);
    }
}