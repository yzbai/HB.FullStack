namespace HB.Framework.Common.Server
{
    public interface ISmsService
    {
        /// <summary>
        /// 用于验证用户手机
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Aliyun.Sms.AliyunSmsException"></exception>
        void SendValidationCode(string mobile/*, out string code*/);

        bool Validate(string mobile, string code);
    }
}