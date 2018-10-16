using HB.Component.Common.Vod.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Common.Vod
{
    public interface IVodBiz
    {
        Task<PlayAuth> GetVideoPlayAuth(string vid);
    }
}
