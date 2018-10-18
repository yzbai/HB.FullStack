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

            PlayAuth playAuth = new PlayAuth();

            playAuth.RequestId = response.RequestId;
            playAuth.Auth = response.PlayAuth;
            playAuth.Title = response.VideoMeta.Title;
            playAuth.VideoId = response.VideoMeta.VideoId;
            playAuth.Status = response.VideoMeta.Status;
            playAuth.CoverURL = response.VideoMeta.CoverURL;
            playAuth.Duration = response.VideoMeta.Duration;

            return playAuth;
        }
    }
}
