using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Identity.Entities;

namespace HB.FullStack.Identity
{
    public interface IIdentityService
    {
        Task AddRolesToUserAsync(long userId, long roleId, string lastUser);
        Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext? transactionContext);
        Task RemoveRoleFromUserAsync(long userId, long roleId, string lastUser);
    }
}