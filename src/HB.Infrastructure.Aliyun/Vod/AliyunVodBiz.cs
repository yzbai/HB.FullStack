using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.vod.Model.V20170321;
using HB.Component.Resource.Vod;
using HB.Component.Resource.Vod.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
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

        public Task<PlayAuth> GetVideoPlayAuth(string vid, long timeout)
        {
            GetVideoPlayAuthRequest request = new GetVideoPlayAuthRequest();
            request.VideoId = vid;
            request.AuthInfoTimeout = timeout;

            return Policy
                .Handle<ServerException>()
                .Or<ClientException>()
                .WaitAndRetryAsync(
                    new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8) },
                    (exception, timeSpan, retryCount, context) =>
                    {
                        ClientException cex = (ClientException)exception;
                        _logger.LogError(exception, "Code:{0}, Msg:{1}, Type:{2}, Msg:{3}", cex.ErrorCode, cex.ErrorMessage, cex.ErrorType.GetDescription(), cex.Message);
                    })
                .ExecuteAsync<PlayAuth>(async () => {
                    var task = Task.Factory.StartNew<GetVideoPlayAuthResponse>(() => _acsClient.GetAcsResponse<GetVideoPlayAuthResponse>(request));
                    var result = await task;

                    PlayAuth playAuth = new PlayAuth();

                    playAuth.RequestId = result.RequestId;
                    playAuth.Auth = result.PlayAuth;
                    playAuth.Title = result.VideoMeta.Title;
                    playAuth.VideoId = result.VideoMeta.VideoId;
                    playAuth.Status = result.VideoMeta.Status;
                    playAuth.CoverURL = result.VideoMeta.CoverURL;
                    playAuth.Duration = result.VideoMeta.Duration;

                    return playAuth;
                });
        }
    }
}
