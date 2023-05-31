﻿using System.Globalization;
using System.Linq;
using System.Security.Claims;

using HB.FullStack.Common.Convert;

namespace System
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetClaimValue(this ClaimsPrincipal principal, string claimExtensionType)
        {
            if (principal.HasClaim(c => c.Type == claimExtensionType))
            {
                Claim claim = principal.Claims.First(c => c.Type == claimExtensionType);

                return claim.Value;
            }

            return null;
        }

        public static TId? GetUserId<TId>(this ClaimsPrincipal principal)
        {
            string? userIdString = principal.GetClaimValue(ClaimExtensionTypes.USER_ID);

            return StringConvertCenter.FromStringTo<TId>(userIdString, StringConvertPurpose.NONE);
        }

        public static string? GetUserIdString(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.USER_ID);
        }

        public static string? GetUserLevel(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.USER_LEVEL);
        }

        public static string? GetUserSecurityStamp(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.SECURITY_STAMP);
        }

        public static string? GetAudience(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.AUDIENCE);
        }

        public static TId? GetTokenCredentialId<TId>(this ClaimsPrincipal principal)
        {
            string? idString = principal.GetClaimValue(ClaimExtensionTypes.TOKEN_CREDENTIAL_ID);

            return StringConvertCenter.FromStringTo<TId>(idString, StringConvertPurpose.NONE);
        }

        public static string? GetClientId(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimExtensionTypes.CLIENT_ID);
        }

        public static string GetLastUser(this ClaimsPrincipal principal)
        {
            string? userIdStr = principal.GetClaimValue(ClaimExtensionTypes.USER_ID);
            string? clientId = principal.GetClaimValue(ClaimExtensionTypes.CLIENT_ID);

            return $"{userIdStr}-{clientId}";
        }

        //public static string GetAuthtoken(this ClaimsPrincipal principal)
        //{
        //    return principal.GetClaimValue(ClaimExtensionTypes.Authtoken);
        //}

        //public static string? GetLoginName(this ClaimsPrincipal principal)
        //{
        //    return principal.GetClaimValue(ClaimExtensionTypes.LoginName);
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

        //    return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value, Globals.Culture);
        //}

        //public static bool GetIsMobileConfirmed(this ClaimsPrincipal principal)
        //{
        //    string value = principal.GetClaimValue(ClaimExtensionTypes.IsMobileConfirmed);

        //    return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value, Globals.Culture);
        //}
    }
}