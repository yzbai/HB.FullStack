using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.vod.Model.V20170321;
using HB.Component.Resource.Vod;
using HB.Component.Resource.Vod.Entity;
using HB.Infrastructure.Aliyun.Vod.Transform;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Vod
{
    public class AliyunVodService : IVodService
    {
        private IAcsClient _acsClient;
        private AliyunVodOptions _options;
        private ILogger _logger;

        public AliyunVodService(IAcsClientManager acsClientManager, IOptions<AliyunVodOptions> options, ILogger<AliyunVodService> logger)
        {
            _options = options.Value;
            _acsClient = acsClientManager.GetAcsClient(_options.ProductName);
            _logger = logger;
        }

        public Task<PlayAuth> GetVideoPlayAuth(string vid, long timeout)
        {
            GetVideoPlayAuthRequest request = new GetVideoPlayAuthRequest();
            request.VideoId = vid;
            request.AuthInfoTimeout = timeout;

            return PolicyManager.Default(_logger).ExecuteAsync<PlayAuth>(async () => {

                Task<GetVideoPlayAuthResponse> task = new Task<GetVideoPlayAuthResponse>(() => _acsClient.GetAcsResponse(request));
                task.Start(TaskScheduler.Default);
                    
                GetVideoPlayAuthResponse result = await task.ConfigureAwait(false);

                return PlayAuthTransformer.Transform(result);

            });
        }
    }
}
