using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared.Context;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    internal partial class IdentityService
    {
        public async Task<UserProfile> GetUserProfileByUserIdAsync(Guid userId, string lastUser)
        {
            TransactionContext  trans = await _transaction.BeginTransactionAsync<UserProfile>().ConfigureAwait(false);

            try
            {
                UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(userId, trans).ConfigureAwait(false);

                if (userProfile == null)
                {
                    userProfile = new UserProfile
                    {
                        Id = userId,
                        Level = null,
                        NickName = Conventions.GetRandomNickName(),
                        Gender = null,
                        BirthDay = null,
                        AvatarFileName = null
                    };

                    await _userProfileRepo.AddAsync(userProfile,lastUser, trans).ConfigureAwait(false);
                }

                await _transaction.CommitAsync(trans).ConfigureAwait(false);

                return userProfile;
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
