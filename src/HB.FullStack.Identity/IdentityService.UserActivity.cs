using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Server.Identity.Models;
using Microsoft.Extensions.Logging;

using static HB.FullStack.Server.Identity.LengthConventions;

namespace HB.FullStack.Server.Identity
{
    public partial class IdentityService
    {
        #region UserActivity

        public async Task RecordUserActivityAsync(Guid? signInCredentialId, Guid? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode)
        {
            if (SerializeUtil.TryToJson(errorCode, out string? resultError))
            {
                if (resultError?.Length > MAX_RESULT_ERROR_LENGTH)
                {
                    _logger.LogWarning("记录UserActivity时，ErrorCode过长，已截断, {ErrorCode}", resultError);

                    resultError = resultError[..MAX_RESULT_ERROR_LENGTH];
                }
            }

            if (arguments != null && arguments.Length > MAX_ARGUMENTS_LENGTH)
            {
                _logger.LogWarning("记录UserActivity时，Arguments过长，已截断, {Arguments}", arguments);

                arguments = arguments[..MAX_ARGUMENTS_LENGTH];
            }

            if (url != null && url.Length > MAX_URL_LENGTH)
            {
                _logger.LogWarning("记录UserActivity时，url过长，已截断, {Url}", url);

                url = url[..MAX_URL_LENGTH];
            }

            UserActivity model = new UserActivity
            {
                SignInCredentialId = signInCredentialId,
                UserId = userId,
                Ip = ip,
                Url = url,
                HttpMethod = httpMethod,
                Arguments = arguments,
                ResultStatusCode = resultStatusCode,
                ResultType = resultType,
                ResultError = resultError
            };

            await _userActivityModelRepo.AddAsync(model, "", null).ConfigureAwait(false);
        }

        #endregion
    }
}
