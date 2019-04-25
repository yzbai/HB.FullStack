﻿using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using HB.Framework.Database.Transaction;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public class ClaimsPrincipalFactory : IClaimsPrincipalFactory
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly IRoleBiz _roleBiz;
        private readonly IUserClaimBiz _userClaimBiz;

        public ClaimsPrincipalFactory(IDatabase database, ILogger<ClaimsPrincipalFactory> logger, IUserClaimBiz userClaims, IRoleBiz roleBiz)
        {
            _db = database;
            _logger = logger;
            _userClaimBiz = userClaims;
            _roleBiz = roleBiz;
        }

        public async Task<IList<Claim>> CreateClaimsAsync(User user, DatabaseTransactionContext transContext = null)
        {
            if (user == null) { return null; }

            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimExtensionTypes.UserGUID, user.Guid),
                new Claim(ClaimExtensionTypes.UserId, user.Id.ToString(GlobalSettings.Culture)),
                new Claim(ClaimExtensionTypes.UserName, user.UserName??""),
                new Claim(ClaimExtensionTypes.MobilePhone, user.Mobile??""),
                new Claim(ClaimExtensionTypes.SecurityStamp, user.SecurityStamp),
                new Claim(ClaimExtensionTypes.IsMobileConfirmed, user.MobileConfirmed.ToString(GlobalSettings.Culture))
            };

            IList<UserClaim> userClaims = await _userClaimBiz.GetUserClaimsByUserIdAsync(user.Id, transContext).ConfigureAwait(false);

            foreach (UserClaim item in userClaims)
            {
                claims.Add(new Claim(item.ClaimType, item.ClaimValue));
            }

            IEnumerable<string> roleNames = await _roleBiz.GetUserRoleNamesAsync(user.Id, transContext).ConfigureAwait(false);

            foreach (string roleName in roleNames)
            {
                claims.Add(new Claim(ClaimExtensionTypes.Role, roleName));
            }

            return claims;
        }
    }
}
