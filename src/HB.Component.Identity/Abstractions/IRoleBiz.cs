using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    public interface IRoleBiz
    {

        //ErrCode AddRole(Role role, string lastUser);
        //ErrCode AddUserToRole(int userId, string roleName, string lastUser);
        //ErrCode DeleteRole(Role role, string lastUser);
        //ErrCode DeleteRoleByRoleName(string RoleName, string lastUser);
        //ErrCode DeleteUserFromRole(int userId, string roleName, string lastUser);
        //IEnumerable<Role> GetRole();
        //Role GetRoleById(int roleId);
        //Role GetRoleByName(string roleName);
        //IList<Role> GetRoleByNames(string[] roleNames);
        //IList<Role> GetRoleByUserId(int userId);
        //bool IsUserInRole(int userId, string roleName);
        //ErrCode SetUserRoles(int userId, string[] roleNames, string lastUser);
        //ErrCode UpdateRole(Role role, string lastUser);
        Task<IEnumerable<string>> GetUserRoleNamesAsync(long userId, DbTransactionContext transContext = null);

        Task<int> GetRoleByNameAsync(string roleName, DbTransactionContext transContext = null);
    }
}