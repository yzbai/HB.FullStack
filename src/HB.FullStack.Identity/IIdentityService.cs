using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Identity.Entities;

namespace HB.FullStack.Identity
{
    public interface IIdentityService
    {
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser);

        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext? transactionContext);

        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser);
    }
}