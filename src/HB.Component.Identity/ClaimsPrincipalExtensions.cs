using System;
using System.Globalization;
using System.Linq;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
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
            return principal.GetClaimValue(ClaimExtensionTypes.UserGuid);
        }

        public static string GetUserSecurityStamp(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.SecurityStamp);
        }

        public static string GetAudience(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.Audience);
        }

        public static string GetSignInTokenGuid(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.SignInTokenGuid);
        }

        public static string GetClientId(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.ClientId);
        }
        
        //public static string GetAuthtoken(this ClaimsPrincipal principal)
        //{
        //    return principal.GetClaimValue(ClaimExtensionTypes.Authtoken);
        //}

        //public static string GetUserName(this ClaimsPrincipal principal)
        //{
        //    return principal.GetClaimValue(ClaimExtensionTypes.UserName);
        //}

        //public static string GetMobile(this ClaimsPrincipal principal)
        //{
        //    return principal.GetClaimValue(ClaimExtensionTypes.MobilePhone);
        //}

        //public static string GetEmail(this ClaimsPrincipal principal)
        //{
        //    return principal.GetClaimValue(ClaimExtensionTypes.Email);
        //}

        //public static bool GetIsEmailConfirmed(this ClaimsPrincipal principal)
        //{
        //    string value = principal.GetClaimValue(ClaimExtensionTypes.IsEmailConfirmed);

        //    return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value, GlobalSettings.Culture);
        //}

        //public static bool GetIsMobileConfirmed(this ClaimsPrincipal principal)
        //{
        //    string value = principal.GetClaimValue(ClaimExtensionTypes.IsMobileConfirmed);

        //    return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value, GlobalSettings.Culture);
        //}

    }
}
