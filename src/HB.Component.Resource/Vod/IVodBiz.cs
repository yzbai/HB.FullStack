using HB.Component.Resource.Vod.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Resource.Vod
{
    public interface IVodBiz
    {
        Task<PlayAuth> GetVideoPlayAuth(string vid, long timeout);
    }
}
