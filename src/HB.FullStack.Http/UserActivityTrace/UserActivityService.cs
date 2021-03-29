using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Server.UserActivityTrace
{
    public class UserActivityService : IUserActivityService
    {
        private readonly UserActivityRepo _userActivityRepo;

        public UserActivityService(UserActivityRepo userActivityRepo)
        {
            _userActivityRepo = userActivityRepo;
        }

        public async Task RecordUserActivityAsync(long? signInTokenId, long? userId, string? ip, string? url, string? arguments, int? resultStatusCode)
        {
            UserActivity userActivity = new UserActivity(signInTokenId, userId, ip, url, arguments, resultStatusCode);

            await _userActivityRepo.AddAsync(userActivity, "", null).ConfigureAwait(false);
        }
    }
}
