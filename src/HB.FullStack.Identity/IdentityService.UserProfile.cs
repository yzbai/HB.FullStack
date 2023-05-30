using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using HB.FullStack.Common.Shared;
using HB.FullStack.Database;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Server.Identity
{
    internal partial class IdentityService<TId>
    {
        public async Task<UserProfile<TId>> GetUserProfileByUserIdAsync(TId userId, string lastUser)
        {
            TransactionContext  trans = await _transaction.BeginTransactionAsync<UserProfile<TId>>().ConfigureAwait(false);

            try
            {
                UserProfile<TId>? userProfile = await _userProfileRepo.GetByUserIdAsync(userId, trans).ConfigureAwait(false);

                if (userProfile == null)
                {
                    userProfile = new UserProfile<TId>
                    {
                        UserId = userId,
                        NickName = Conventions.GetRandomNickName(),
                        Gender = null,
                        BirthDay = null,
                        AvatarFileName = null
                    };

                    await _userProfileRepo.AddAsync(userProfile,lastUser, trans).ConfigureAwait(false);
                }

                await trans.CommitAsync().ConfigureAwait(false);

                return userProfile;
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task UpdateUserProfileAsync(PropertyChangePack cp, string lastUser)
        {
            TransactionContext trans = await _transaction.BeginTransactionAsync<UserProfile<TId>>().ConfigureAwait(false);

            try
            {
                await _userProfileRepo.UpdateProperties<UserProfile<TId>>(cp, lastUser, trans);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }

        }
    }
}
