using HB.Component.Identity.Entities;
using HB.FullStack.Business;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.SQL;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    /// <summary>
    /// userGuid: Roles
    /// </summary>
    internal class UserRolesCacheItem : CacheItem<IEnumerable<Role>>
    {
        private const string _prefix = "Role";

        public UserRolesCacheItem(string userGuid) : base(_prefix + userGuid) { }
        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;
    }

    internal class RoleBiz
    {
        private readonly IDatabase _database;
        private readonly ICache _cache;

        public RoleBiz(IDatabase database, ICache cache)
        {
            _database = database;
            _cache = cache;
        }

        public async Task<IEnumerable<Role>> GetRolesByUserGuidAsync(string userGuid, TransactionContext? transContext = null)
        {
            //Cache First
            UserRolesCacheItem cacheItem = new UserRolesCacheItem(userGuid);

            if (await _cache.TryGetItemAsync(cacheItem).ConfigureAwait(false))
            {
                return cacheItem.Value!;
            }

            //Cache Missed



            var from = _database.From<Role>().RightJoin<RoleOfUser>((r, ru) => r.Guid == ru.RoleGuid);
            var where = _database.Where<Role>().And<RoleOfUser>(ru => ru.UserGuid == userGuid);

            return await _database.RetrieveAsync(from, where, transContext).ConfigureAwait(false);
        }

        public static void AddRolesToUser()
        {
            // Invalidate Cache : Roles_{UserGuidValue}
        }

        public static void DeleteRolesFromUser()
        {
            //Invalidate Cache : Roles_{UserGuidValue}
        }
    }
}
