using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public class ClaimsPrincipalFactory : BizWithDbTransaction, IClaimsPrincipalFactory
    {
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private IRoleBiz _roleBiz;
        private IUserClaimBiz _userClaimBiz;

        public ClaimsPrincipalFactory(IDatabase database, ILogger<ClaimsPrincipalFactory> logger, IUserClaimBiz userClaims, IRoleBiz roleBiz) 
            : base(database)
        {
            _db = database;
            _logger = logger;
            _userClaimBiz = userClaims;
            _roleBiz = roleBiz;
        }

        public async Task<IList<Claim>> CreateClaimsAsync(User user, DbTransactionContext transContext = null)
        {
            if (user == null) { return null; }

            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimExtensionTypes.UserGUID, user.Guid),
                new Claim(ClaimExtensionTypes.UserId, user.Id.ToString(_culture)),
                new Claim(ClaimExtensionTypes.UserName, user.UserName??""),
                new Claim(ClaimExtensionTypes.MobilePhone, user.Mobile??""),
                new Claim(ClaimExtensionTypes.SecurityStamp, user.SecurityStamp),
                new Claim(ClaimExtensionTypes.IsMobileConfirmed, user.MobileConfirmed.ToString(_culture))
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
