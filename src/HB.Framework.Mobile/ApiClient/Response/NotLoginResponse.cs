using HB.Framework.Common.Mobile;
using System;

namespace HB.Framework.Mobile.ApiClient
{
    public class NotLoginResponse : ApiResponse
    {
        public NotLoginResponse()
             : base(400, "", ErrorCode.API_NOT_LOGIN_YET) { }
    }
}
