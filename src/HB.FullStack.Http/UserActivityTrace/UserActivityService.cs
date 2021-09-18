using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.WebApi.UserActivityTrace
{
    public class UserActivityService : IUserActivityService
    {
        private readonly UserActivityRepo _userActivityRepo;

        public UserActivityService(UserActivityRepo userActivityRepo)
        {
            _userActivityRepo = userActivityRepo;
        }

        public async Task RecordUserActivityAsync(Guid? signInTokenId, Guid? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode)
        {
            UserActivity userActivity = new UserActivity(signInTokenId, userId, ip, url, httpMethod, arguments, resultStatusCode, resultType, errorCode);

            await _userActivityRepo.AddAsync(userActivity, "", null).ConfigureAwait(false);
        }
    }
}
