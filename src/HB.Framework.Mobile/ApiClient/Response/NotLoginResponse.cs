using HB.Framework.Common.Api;

namespace HB.Framework.Client.ApiClient
{
    public class NotLoginResponse : ApiResponse
    {
        public NotLoginResponse()
             : base(400, "", ApiError.API_NOT_LOGIN_YET) { }
    }
}
