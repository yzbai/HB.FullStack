using System;

namespace HB.Framework.Http.SDK
{
    public class NotLoginResponse<T> : Resource<T> where T:ResourceResponse
    {
        public NotLoginResponse()
             : base(400, "", ErrorCode.API_NOT_LOGIN_YET) { }
    }
}
