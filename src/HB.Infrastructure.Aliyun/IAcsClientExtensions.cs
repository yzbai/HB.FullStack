using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aliyun.Acs.Core;

namespace Aliyun.Acs.Core
{
    public static class IAcsClientExtensions
    {
        //TODO: 等待阿里云增加异步方法
        public static Task<CommonResponse> GetCommonResponseAsync(this IAcsClient client, CommonRequest request)
        {
            return Task.Run(() => client.GetCommonResponse(request));
        }
    }
}
