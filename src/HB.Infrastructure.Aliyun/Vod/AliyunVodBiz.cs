using Aliyun.Acs.Core;
using HB.Component.Common.Vod;
using HB.Component.Common.Vod.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Vod
{
    public class AliyunVodBiz : IVodBiz
    {
        private IAcsClient _acsClient;
        private AliyunVodOptions _options;
        private ILogger _logger;

        public AliyunVodBiz(IAcsClientManager acsClientManager, IOptions<AliyunVodOptions> options, ILogger<AliyunVodBiz> logger)
        {
            _options = options.Value;
            _acsClient = acsClientManager.GetAcsClient(_options.ProductName);
            _logger = logger;
        }

        public Task<PlayAuth> GetVideoPlayAuth(string vid)
        {
            throw new NotImplementedException();
        }
    }
}
