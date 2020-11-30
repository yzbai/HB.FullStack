using HB.Component.Identity.Biz;
using HB.Component.Identity.Entities;
using HB.FullStack.Database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class ClaimsPrincipalFactory
    {
        private readonly RoleOfUserBiz _roleOfUserBiz;
        private readonly UserClaimBiz _userClaimBiz;

        public ClaimsPrincipalFactory(UserClaimBiz userClaims, RoleOfUserBiz roleOfUserBiz)
        {
            _userClaimBiz = userClaims;
            _roleOfUserBiz = roleOfUserBiz;
        }

        /// <summary>
        /// 在Claims中放入UserGuid, SecurityStamp, UserClaim表中声明加入JWT的, 所有roleName 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Claim>> CreateClaimsAsync(User user, TransactionContext transContext)
        {
            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimExtensionTypes.UserGuid, user.Guid),
                new Claim(ClaimExtensionTypes.SecurityStamp, user.SecurityStamp),
                //new Claim(ClaimExtensionTypes.UserId, user.Id.ToString(GlobalSettings.Culture)),
                new Claim(ClaimExtensionTypes.LoginName, user.LoginName??""),
                //new Claim(ClaimExtensionTypes.MobilePhone, user.Mobile??""),
                //new Claim(ClaimExtensionTypes.IsMobileConfirmed, user.MobileConfirmed.ToString(GlobalSettings.Culture))
            };

            IEnumerable<UserClaim> userClaims = await _userClaimBiz.GetByUserGuidAsync(user.Guid, transContext).ConfigureAwait(false);

            userClaims.ForEach(item =>
            {
                if (item.AddToJwt)
                {
                    claims.Add(new Claim(item.ClaimType, item.ClaimValue));
                }
            });

            IEnumerable<Role> roles = await _roleOfUserBiz.GetRolesByUserGuidAsync(user.Guid, transContext).ConfigureAwait(false);

            roles.Select(r => r.Name).ForEach(roleName =>
            {
                claims.Add(new Claim(ClaimExtensionTypes.Role, roleName));
            });

            return claims;

            ////并行
            ///不能用并行 https://mysqlconnector.net/troubleshooting/connection-reuse/
            //return TaskUtil.Concurrence(

            //    _userClaimBiz.GetAsync<TUserClaim>(user.Guid, transContext),

            //    _roleBiz.GetByUserGuidAsync<TRole, TRoleOfUser>(user.Guid, transContext),

            //    userClaims =>
            //    {
            //        List<Claim> rts = new List<Claim>();
            //        userClaims.ForEach((item) =>
            //        {
            //            if (item.AddToJwt)
            //            {
            //                rts.Add(new Claim(item.ClaimType, item.ClaimValue));
            //            }
            //        });
            //        return rts;
            //    },

            //    roles =>
            //    {
            //        List<Claim> rts = new List<Claim>();

            //        roles.Select(r => r.Name).ForEach(roleName => rts.Add(new Claim(ClaimExtensionTypes.Role, roleName)));

            //        return rts;
            //    });
        }
    }
}
