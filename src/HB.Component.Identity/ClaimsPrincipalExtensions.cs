using System;
using System.Globalization;
using System.Linq;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        public static string GetClaimValue(this ClaimsPrincipal principal, string claimExtensionType)
        {
            if (principal.HasClaim(c => c.Type == claimExtensionType))
            {
                Claim claim = principal.Claims.First(c => c.Type == claimExtensionType);

                return claim.Value;
            }

            return null;
        }

        public static string GetUserGuid(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.UserGUID);
        }

        public static string GetUserTokenIdentifier(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.SignInTokenIdentifier);
        }

        public static string GetAuthtoken(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.Authtoken);
        }

        public static string GetUserSecurityStamp(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.SecurityStamp);
        }

        public static long GetUserId(this ClaimsPrincipal principal)
        {
            string value = principal.GetClaimValue(ClaimExtensionTypes.UserId);

            return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt64(value, _culture);
        }

        public static string GetAudience(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.Audience);
        }

        public static string GetSignInTokenIdentifier(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.SignInTokenIdentifier);
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.UserName);
        }

        public static string GetMobile(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.MobilePhone);
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.Email);
        }

        public static bool GetIsEmailConfirmed(this ClaimsPrincipal principal)
        {
            string value = principal.GetClaimValue(ClaimExtensionTypes.IsEmailConfirmed);

            return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value, _culture);
        }

        public static bool GetIsMobileConfirmed(this ClaimsPrincipal principal)
        {
            string value = principal.GetClaimValue(ClaimExtensionTypes.IsMobileConfirmed);

            return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value, _culture);
        }
    }
}
