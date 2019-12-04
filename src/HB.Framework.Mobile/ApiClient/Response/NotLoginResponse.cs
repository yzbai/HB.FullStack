using HB.Framework.Common.Api;

namespace HB.Framework.Mobile.ApiClient
{
    public class NotLoginResponse : ApiResponse
    {
        public NotLoginResponse()
             : base(400, "", ApiError.API_NOT_LOGIN_YET) { }
    }
}
