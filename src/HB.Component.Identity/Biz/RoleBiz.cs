using HB.Component.Identity.Entities;
using HB.FullStack.Business;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class RoleBiz : EntityBaseBiz<Role>
    {
        private readonly IDatabaseReader _databaseReader;

        public RoleBiz(ILogger<RoleBiz> logger, IDatabaseReader databaseReader, ICache cache) : base(logger, databaseReader, cache)
        {
            _databaseReader = databaseReader;
        }

        public async Task<IEnumerable<Role>?> GetRolesByUserGuidAsync(string userGuid, TransactionContext? transContext = null)
        {

            //return TryCacheAsideLooseAsync<CachedRolesByUserGuid, IEnumerable<Role>>(db =>
            //{
            //    var from = _databaseReader.From<Role>().RightJoin<RoleOfUser>((r, ru) => r.Guid == ru.RoleGuid);
            //    var where = _databaseReader.Where<Role>().And<RoleOfUser>(ru => ru.UserGuid == userGuid);

            //    return _databaseReader.RetrieveAsync(from, where, transContext);
            //}, userGuid);

            //Cache First
            IEnumerable<Role>? roles = await CachedRolesByUserGuid.Key(userGuid).GetFromAsync(_cache).ConfigureAwait(false);

            if (roles != null)
            {
                return roles;
            }

            //Cache Missed Retrieve Database
            var from = _databaseReader.From<Role>().RightJoin<RoleOfUser>((r, ru) => r.Guid == ru.RoleGuid);
            var where = _databaseReader.Where<Role>().And<RoleOfUser>(ru => ru.UserGuid == userGuid);

            IEnumerable<Role> results = await _databaseReader.RetrieveAsync(from, where, transContext).ConfigureAwait(false);

            //Update Cache
            CachedRolesByUserGuid.Key(userGuid).Value(results).SetToAsync(_cache).Fire();

            //Return
            return results;

            ////doble check
            //roles = await RolesByUserGuidCacheItem.TryGetValueAsync(_cache, userGuid).ConfigureAwait(false);

            //if (roles != null)
            //{
            //    return roles;
            //}

            //roles = retrieve();

            //RolesByUserGuidCacheItem.SetAsync(_cache, userGuid, roles);






        }

        public static void AddRolesToUser()
        {
            // Invalidate Cache : Roles_{UserGuidValue}

            CachedRolesByUserGuid.Remove(_cache, userGuid);
        }

        public static void DeleteRolesFromUser()
        {
            //Invalidate Cache : Roles_{UserGuidValue}
        }
    }
}
