using System.ComponentModel.DataAnnotations;

using AsyncAwaitBestPractices;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HB.FullStack.Server.WebLib.MinimalApis
{
    public static class SmsValidationCodeApi
    {
        public static RouteGroupBuilder MapSmsValidationCodeApi(this RouteGroupBuilder group)
        {
            group.MapGet(SharedNames.Conditions.ByMobile, ([FromQuery][Mobile] string mobile, [FromServices] ISmsService smsService) =>
            {
#if DEBUG
                smsService.SendValidationCodeAsync(mobile, "1111", int.MaxValue).SafeFireAndForget();

                return Results.Ok(new SmsValidationCodeRes { Length = 4 });
#else

            int smsCodeLength = smsService.SendValidationCode(mobile);

            return Results.Ok(new SmsValidationCodeRes { Length = smsCodeLength });
#endif
            }).AllowAnonymous();

            //TODO: 将CapthaFilter 抽象出来，不能只依赖腾讯一家
            //[ServiceFilter(typeof(CapthcaCheckFilter))]
            //.AddEndpointFilter<CapthcaCheckFilter>;

            return group;
        }
    }
}
