using Aliyun.Acs.vod.Model.V20170321;
using HB.Component.Resource.Vod.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Vod.Transform
{
    public static class PlayAuthTransformer
    {
        public static PlayAuth Transform(GetVideoPlayAuthResponse response)
        {
            if (response == null)
            {
                return null;
            }

            PlayAuth playAuth = new PlayAuth
            {
                RequestId = response.RequestId,
                Auth = response.PlayAuth,
                Title = response.VideoMeta.Title,
                VideoId = response.VideoMeta.VideoId,
                Status = response.VideoMeta.Status,
                CoverURL = response.VideoMeta.CoverURL,
                Duration = response.VideoMeta.Duration
            };

            return playAuth;
        }
    }
}
