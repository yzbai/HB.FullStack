using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;
using HB.FullStack.Database;
using HB.FullStack.KVStore;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    internal class UserLoginControlRepo
    {
        private readonly IKVStore _kv;
        public UserLoginControlRepo(IKVStore kvStore)
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

            if (uc.Version == -1)
            {
                await _kv.AddAsync(uc, lastUser).ConfigureAwait(false);
            }
            else
            {
                await _kv.UpdateAsync(uc, lastUser).ConfigureAwait(false);
            }
        }

        public async Task SetAccessFailedCountAsync(string userGuid, long count, string lastUser)
        {
            UserLoginControl? uc = await _kv.GetAsync<UserLoginControl>(userGuid).ConfigureAwait(false);

            if (uc == null)
            {
                uc = new UserLoginControl { UserGuid = userGuid };
            }

            uc.LoginFailedCount = count;

            if (uc.Version == -1)
            {
                await _kv.AddAsync(uc, lastUser).ConfigureAwait(false);
            }
            else
            {
                await _kv.UpdateAsync(uc, lastUser).ConfigureAwait(false);
            }
        }

        internal async Task<UserLoginControl> GetOrCreateByUserGuidAsync(string userGuid)
        {
            UserLoginControl? uc = await _kv.GetAsync<UserLoginControl>(userGuid).ConfigureAwait(false);

            if (uc == null)
            {
                uc = new UserLoginControl();

                await _kv.AddAsync(uc, "default").ConfigureAwait(false);
            }

            return uc;
        }

        internal Task UpdateAsync(UserLoginControl userLoginControl, string lastUser)
        {
            return _kv.UpdateAsync<UserLoginControl>(userLoginControl, lastUser);
        }
    }
}
