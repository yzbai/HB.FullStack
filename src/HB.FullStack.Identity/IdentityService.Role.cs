using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Server.Identity
{
    public partial class IdentityService
    {
        #region Role

        //        public async Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser)
        //        {
        //            //TODO: 需要重新构建 jwt

        //            ThrowIf.Empty(ref userId, nameof(userId));
        //            ThrowIf.Empty(ref roleId, nameof(roleId));

        //            TransactionContext trans = await _transaction.BeginTransactionAsync<UserRole>().ConfigureAwait(false);
        //            try
        //            {
        //                //查重
        //                IEnumerable<Role> storeds = await _roleRepo.GetByUserIdAsync(userId, trans).ConfigureAwait(false);

        //                if (storeds.Any(ur => ur.Id == roleId))
        //                {
        //                    throw IdentityExceptions.FoundTooMuch(userId: userId, roleId: roleId, cause: "已经有相同的角色");
        //                }



        //                UserRole ru = new UserRole(userId, roleId);

        //                await _userRoleRepo.AddAsync(ru, lastUser, trans).ConfigureAwait(false);

        //                await trans.CommitAsync().ConfigureAwait(false);
        //            }
        //            catch
        //            {
        //                await trans.RollbackAsync().ConfigureAwait(false);
        //                throw;
        //            }
        //        }

        //        public async Task<bool> TryRemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser)
        //        {
        //            //需要重新构建 jwt

        //            TransactionContext trans = await _transaction.BeginTransactionAsync<UserRole>().ConfigureAwait(false);

        //            try
        //            {
        //                //查重
        //                IEnumerable<Role> storeds = await _roleRepo.GetByUserIdAsync(userId, trans).ConfigureAwait(false);

        //                Role? stored = storeds.SingleOrDefault(ur => ur.Id == roleId);

        //                if (stored == null)
        //                {
        //                    return false;
        //                }

        //                UserRole? userRole = await _userRoleRepo.GetByUserIdAndRoleIdAsync(userId, roleId, trans).ConfigureAwait(false);

        //                await _userRoleRepo.DeleteAsync(userRole!, lastUser, trans).ConfigureAwait(false);

        //                await trans.CommitAsync().ConfigureAwait(false);

        //                return true;
        //            }
        //#pragma warning disable CA1031 // Do not catch general exception types
        //            catch (Exception ex)
        //#pragma warning restore CA1031 // Do not catch general exception types
        //            {
        //                await trans.RollbackAsync().ConfigureAwait(false);

        //                _logger.LogTryRemoveRoleFromUserError(userId, roleId, lastUser, ex);

        //                return false;
        //            }
        //        }

        #endregion
    }
}
