using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entities;
using HB.Framework.Database;
using HB.Framework.KVStore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity.Biz
{
    internal class UserLoginControlBiz : IUserLoginControlBiz
    {
        private readonly IKVStore _kv;
        public UserLoginControlBiz(IKVStore kvStore)
        {
            _kv = kvStore;
        }

        public async Task SetLockoutAsync(string userGuid, bool lockout, string lastUser, TimeSpan? lockoutTimeSpan = null)
        {
            UserLoginControl? uc = await _kv.GetAsync<UserLoginControl>(userGuid).ConfigureAwait(false);

            if (uc == null)
            {
                uc = new UserLoginControl { UserGuid = userGuid };
            }

            uc.LockoutEnabled = lockout;
            uc.LockoutEndDate = DateTimeOffset.UtcNow + (lockoutTimeSpan ?? TimeSpan.FromDays(1));

            await _kv.AddOrUpdateAsync(uc, lastUser).ConfigureAwait(false);
        }

        public async Task SetAccessFailedCountAsync(string userGuid, long count, string lastUser)
        {
            UserLoginControl? uc = await _kv.GetAsync<UserLoginControl>(userGuid).ConfigureAwait(false);

            if (uc == null)
            {
                uc = new UserLoginControl { UserGuid = userGuid };
            }

            uc.LoginFailedCount = count;

            await _kv.AddOrUpdateAsync(uc, lastUser).ConfigureAwait(false);
        }
    }
}
