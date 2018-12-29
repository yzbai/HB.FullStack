using System;
using System.Collections.Generic;
using System.Text;
using Aliyun.Acs.Sts.Model.V20150401;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class AliyunOssService
    {
        public AliyunOssService(IOptions<AliyunOssOptions> options, IAcsClientManager acsClientManager, ILogger<AliyunOssService> logger)
        {

        }

        public AssumeRoleResponse GetToken
    }
}
