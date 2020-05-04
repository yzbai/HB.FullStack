using HB.Framework.Common.Api;

namespace HB.Framework.Client.Api
{
    public class NotLoginResponse : ApiResponse
    {
        public NotLoginResponse()
             : base(400, "", ApiError.ApiNotLoginYet) { }
    }
}
