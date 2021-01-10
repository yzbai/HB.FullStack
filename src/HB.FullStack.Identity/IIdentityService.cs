using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Identity.Entities;

namespace HB.FullStack.Identity
{
    public interface IIdentityService
    {
        /// <exception cref="HB.FullStack.Identity.IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task AddRolesToUserAsync(long userId, long roleId, string lastUser);

        /// <exception cref="HB.FullStack.Identity.IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext? transactionContext);

        /// <exception cref="HB.FullStack.Identity.IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task RemoveRoleFromUserAsync(long userId, long roleId, string lastUser);
    }
}